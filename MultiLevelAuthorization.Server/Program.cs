using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Server.Models;

namespace MultiLevelAuthorization.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        //enable cross-origin; MUST before anything
        builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", corsPolicyBuilder =>
        {
            corsPolicyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetPreflightMaxAge(TimeSpan.FromHours(24 * 30));
        }));

        // Add authentications
        var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration.GetValue<string>("AuthenticationKey")));
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer("Robot", options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RequireSignedTokens = true,
                    IssuerSigningKey = securityKey,
                    ValidAudiences = new[] { Application.Issuer },
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(TokenValidationParameters.DefaultClockSkew.TotalSeconds),
                };
            });

        // Add services to the container.
        builder.Services.AddControllers(options =>
        {
            options.ModelMetadataDetailsProviders.Add(new SuppressChildValidationMetadataProvider(typeof(IPAddress)));
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddAppSwaggerGen(Application.Name);
        builder.Services.AddMemoryCache();
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
        builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Database")));
        builder.Services.AddSingleton<Application>();

        // Create TimedHostedService
        builder.Services.AddHostedService<TimedHostedService>();

        var webApp = builder.Build();

        // Cors must configure before any Authorization to allow token request
        webApp.UseCors("CorsPolicy");

        // Configure the HTTP request pipeline.
        webApp.UseSwagger();
        webApp.UseSwaggerUI();

        webApp.UseHttpsRedirection();
        webApp.UseAuthorization();
        webApp.MapControllers();

        var application = webApp.Services.GetRequiredService<Application>();
        await application.Run(webApp, args);
    }
}
