using System.Text;
using BandCommunity.Application.Services.Auth;
using BandCommunity.Domain.Entities;
using BandCommunity.Domain.JWT;
using BandCommunity.Infrastructure.Auth;
using BandCommunity.Infrastructure.Data;
using BandCommunity.Application.Services.Role;
using dotenv.net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        
        builder.Services.AddIdentity<User, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.Configure<Jwt>(options =>
        {
            options.Secret = Environment.GetEnvironmentVariable("JWT_SECRET")!;
            options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
            options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
            options.ExpiryInMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "15");
        });

        //* Add Bearer Authentication
        var jwtOptions = builder.Configuration.GetSection(Jwt.JWTOptionsKey).Get<Jwt>();

        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions!.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.Secret))
                };
            });

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
        
        //* Google Authentication
        builder.Services.AddAuthentication()
            .AddCookie()
            .AddGoogle(options =>
            {
                var clientId = options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID") ?? String.Empty;

                if (string.IsNullOrEmpty(clientId))
                {
                    throw new Exception("Google Client ID is not set in environment variables.");
                }
                
                var clientSecret = options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET") ?? String.Empty;
                
                if (string.IsNullOrEmpty(clientSecret))
                {
                    throw new Exception("Google Client Secret is not set in environment variables.");
                }
                
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.ClaimActions.MapJsonKey("profile_picture", "profile_picture");
                options.SaveTokens = true;
                options.CallbackPath = "/signin-google";
            });
        
        //* Redis Cache
        builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
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
        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
        });
        
        //* Add services to the container.
        builder.Services.AddAuthorization();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllers();

        //* Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

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

        app.UseAuthentication();
        app.UseAuthorization();

        app.Run();
    }
}