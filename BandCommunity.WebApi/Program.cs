using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using BandCommunity.Application.Services.Auth;
using BandCommunity.Application.Services.Email;
using BandCommunity.Domain.Entities;
using BandCommunity.Domain.JWT;
using BandCommunity.Infrastructure.Auth;
using BandCommunity.Infrastructure.Data;
using BandCommunity.Application.Services.Role;
using BandCommunity.Domain.Interfaces;
using BandCommunity.Infrastructure.Repositories;
using BandCommunity.Shared.Utility;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;

namespace BandCommunity.WebApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        DotEnv.Load();

        var builder = WebApplication.CreateBuilder(args);

        var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        //! IMPORTANT: Add this line to enable lazy loading
        builder.Services.AddScoped<IAuthTokenProcess, AuthTokenProcess>();
        builder.Services.AddScoped<IAuthorizeService, AuthorizeService>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddTransient<IEmailSender, EmailSender>();
        builder.Services.AddHttpClient<CountryStateService>();

        builder.Services.AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        //* Add Bearer Authentication
        builder.Services.Configure<Jwt>(options =>
        {
            options.Secret = Environment.GetEnvironmentVariable("JWT_SECRET")!;
            options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
            options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
            options.ExpiryInMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "15");
        });

        //* CORS Configuration
        var myAllowSpecificOrigins = "AllowAllOrigins";
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: myAllowSpecificOrigins,
                policy =>
                {
                    policy.WithOrigins(Environment.GetEnvironmentVariable("FRONTEND_URL")!)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        //* Authentication
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            })
            .AddJwtBearer("Bearer", _ =>
            { });
        
        builder.Services.PostConfigure<JwtBearerOptions>("Bearer", options =>
        {
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
            var secret = Environment.GetEnvironmentVariable("JWT_SECRET")!;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ClockSkew = TimeSpan.Zero, //* Disable the default 5-minute clock skew
                RequireExpirationTime = true //* Require the token to have an expiration time
            };
        });

        //* Redis Cache
        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");

            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                throw new InvalidOperationException("Redis connection string is not set in environment variables.");
            }

            var configurationOptions = ConfigurationOptions.Parse(redisConnectionString, true);
            configurationOptions.AbortOnConnectFail = false;

            try
            {
                return ConnectionMultiplexer.Connect(configurationOptions);
            }
            catch (RedisConnectionException ex)
            {
                throw new InvalidOperationException("Failed to connect to Redis: " + ex.Message, ex);
            }
        });


        builder.Services.AddScoped<IDatabase>(sp =>
        {
            var connectionMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            return connectionMultiplexer.GetDatabase();
        });

        //* Email Confirmation
        builder.Services.Configure<IdentityOptions>(options => { options.SignIn.RequireConfirmedEmail = true; });

        //* Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy =
                    JsonNamingPolicy.CamelCase; //* Use original property names
                options.JsonSerializerOptions.PropertyNameCaseInsensitive =
                    true; //* Enable case-insensitive property names
            });

        //* Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "BandCommunity API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Please enter a valid token"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        });

        var app = builder.Build();

        //* Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //* Seed role 
        using (var scope = app.Services.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            await RoleServices.SeedRole(roleManager);
        }

        app.UseCors(myAllowSpecificOrigins);
        app.UseHttpsRedirection();

        app.UseRouting();

        //* Middleware to handle JWT token validation
        app.Use(async (context, next) =>
        {
            var token = context.Request.Cookies["ACCESS_TOKEN"];
            if (!string.IsNullOrEmpty(token))
            {
                var jwtOptions = new Jwt
                {
                    Secret = Environment.GetEnvironmentVariable("JWT_SECRET")!,
                    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!,
                    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                    ClockSkew = TimeSpan.Zero,
                    RequireExpirationTime = true
                };

                try
                {
                    var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                    context.User = principal;
                }
                catch
                {
                    //* Invalid token, do not set context.User
                }
            }

            await next();
        });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}