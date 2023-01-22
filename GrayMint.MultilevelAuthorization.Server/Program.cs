using GrayMint.Common.AspNetCore;
using GrayMint.Common.AspNetCore.Auth.BotAuthentication;
using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
using Microsoft.EntityFrameworkCore;
using MultiLevelAuthorization.Persistence;
using MultiLevelAuthorization.Repositories;
using MultiLevelAuthorization.Services;

namespace MultiLevelAuthorization.Server;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Config core requirements
        builder.AddGrayMintCommonServices(builder.Configuration.GetSection("App"), new RegisterServicesOptions());
        builder.Services
            .AddAuthentication()
            .AddBotAuthentication(builder.Configuration.GetSection("Auth"), builder.Environment.IsProduction());

        // Config core requirements
        builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDatabase") ?? throw new InvalidOperationException()));
        builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("App"));
        builder.Services.AddScoped<AppService>();
        builder.Services.AddScoped<RoleService>();
        builder.Services.AddScoped<SecureObjectService>();
        builder.Services.AddScoped<PermissionService>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<AuthRepo>();
        builder.Services.AddScoped<ISimpleRoleProvider, SimpleRoleProvider>();
        builder.Services.AddSimpleRoleAuthorization(builder.Configuration.GetSection("Auth"), true, false);

        var webApp = builder.Build();
        webApp.UseGrayMintCommonServices(new UseServicesOptions());
        await GrayMintApp.CheckDatabaseCommand<AuthDbContext>(webApp, args);

        // Initialize db
        await using (var scope = webApp.Services.CreateAsyncScope())
        {
            var authRepo = scope.ServiceProvider.GetRequiredService<AuthRepo>();
            await authRepo.ExecuteSqlRaw(authRepo.SecureObject_HierarchySql());
        }

        await GrayMintApp.RunAsync(webApp, args);
    }
}
