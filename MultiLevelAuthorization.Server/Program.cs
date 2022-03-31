using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Persistence;
using MultiLevelAuthorization.Server.Mapper;
using MultiLevelAuthorization.ServiceRegistration;

namespace MultiLevelAuthorization.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigurationManager configuration = builder.Configuration;

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
        var key = Convert.FromBase64String(builder.Configuration.GetValue<string>("App:AuthenticationKey"));
        var securityKey = new SymmetricSecurityKey(key);
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(AppOptions.AuthRobotScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RequireSignedTokens = true,
                    IssuerSigningKey = securityKey,
                    ValidIssuer = AppOptions.AuthIssuer,
                    ValidAudience = AppOptions.AuthIssuer,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
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
        builder.Services.AddAppSwaggerGen(AppOptions.Name);
        builder.Services.AddMemoryCache();
        builder.Host.ConfigureServices(services => ConfigureServices(configuration, services));

        builder.Services.AddHostedService<TimedHostedService>();
        
        builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("App"));

        var mapperConfig = new MapperConfiguration(mc =>
        {
            mc.AddProfile(new AppCreateRequestMapper());
        });

        IMapper mapper = mapperConfig.CreateMapper();
        builder.Services.AddSingleton(mapper);
        //---------------------
        // Create App
        //---------------------
        var webApp = builder.Build();
        var logger = webApp.Services.GetRequiredService<ILogger<Program>>();


        // Cors must configure before any Authorization to allow token request
        webApp.UseCors("CorsPolicy");

        // Configure the HTTP request pipeline.
        webApp.UseSwagger();
        webApp.UseSwaggerUI();

        webApp.UseHttpsRedirection();
        webApp.UseAuthorization();
        webApp.MapControllers();

        //---------------------
        // Initializing App
        //---------------------
        using var scope = webApp.Services.CreateScope();
        if (args.Contains("/recreatedb"))
        {
            logger.LogInformation($"Recreating the {nameof(AuthDbContext)} database...");
            var appDbContext2 = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            await appDbContext2.Database.EnsureDeletedAsync();
            await appDbContext2.Database.EnsureCreatedAsync();
            return;
        }

        await webApp.RunAsync();
    }

    public static string CreateAppCreatorToken(byte[] key)
    {
        return JwtTool.CreateSymmetricJwt(key, AppOptions.AuthIssuer, AppOptions.AuthIssuer, Guid.NewGuid().ToString(), null, new[] { "AppCreator" });
    }

    static void ConfigureServices(IConfiguration configuration,
   IServiceCollection services)
    {
        services.AddInfrastructureServices(configuration);
    }

}
