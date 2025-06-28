using System.Text;
using BandCommunity.Application.Services.Auth;
using BandCommunity.Domain.Entities;
using BandCommunity.Domain.JWT;
using BandCommunity.Infrastructure.Auth;
using BandCommunity.Infrastructure.Data;
using dotenv.net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BandCommunity.WebApi;

public class Program
{
    public static void Main(string[] args)
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

        
        //* Add services to the container.
        builder.Services.AddAuthorization();
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

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.Run();
    }
}