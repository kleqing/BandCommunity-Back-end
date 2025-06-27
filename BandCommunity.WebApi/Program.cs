using BandCommunity.Infrastructure.Data;
using dotenv.net;
using Microsoft.EntityFrameworkCore;

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
        
        // builder.Services.Configure<JwtOptions>(options =>
        // {
        //     options.Secret = Environment.GetEnvironmentVariable("JWT_SECRET");
        //     options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        //     options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        //     options.ExpiryInMinutes = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRY_MINUTES") ?? "15");
        // });
        
        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.Run();
    }
}