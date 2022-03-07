
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Server.Controllers;
using MultiLevelAuthorization.Server.Models;

namespace MultiLevelAuthorization.Server;

public class Application
{
    private readonly IConfiguration _configuration;
    public static string Name => "Authorization Server";
    public static string Issuer => "authorization-server";

    //private bool _designMode;
    //private bool _recreateDb;
    //private bool _testMode;
    public ILogger<Application> Logger { get; }
    public WebApplication WebApp { get; private set; } = default!;
    public bool AutoMaintenance { get; set; }
    public byte[] AuthenticationKey => Convert.FromBase64String(_configuration.GetValue<string>("AuthenticationKey"));

    public Application(ILogger<Application> logger, IConfiguration configuration)
    {
        _configuration = configuration;
        Logger = logger;
    }

    public string CreateAppCreatorToken()
    {
        return JwtTool.CreateSymmetricJwt(AuthenticationKey, Issuer, Issuer, Guid.NewGuid().ToString(), null, new[] { "AppCreator" });
    }

    public async Task Run(WebApplication webApp, string[] args)
    {
        // recreate database
        if (args.Contains("/recreatedb"))
        {
            using var scope = webApp.Services.CreateScope();
            Logger.LogInformation("Recreating the main database...");

            var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await appDbContext.Database.EnsureDeletedAsync();
            await appDbContext.Database.EnsureCreatedAsync();

            var appDbContext2 = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
            var databaseCreator = (RelationalDatabaseCreator)appDbContext2.Database.GetService<IDatabaseCreator>();
            await databaseCreator.CreateTablesAsync();
            return;
        }

        WebApp = webApp;
        await webApp.RunAsync();
    }
}