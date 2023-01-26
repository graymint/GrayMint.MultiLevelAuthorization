using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GrayMint.Common.AspNetCore.Auth.BotAuthentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MultiLevelAuthorization.Persistence;
using MultiLevelAuthorization.Server;
using MultiLevelAuthorization.Test.Api;

namespace MultiLevelAuthorization.Test.Helper;

public class TestInit
{
    public IServiceScope Scope { get; }
    public AuthDbContext AuthDbContext => Scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    public int AppId { get; private set; }
    public WebApplicationFactory<Program> WebApp { get; set; }
    public HttpClient HttpClientAppUser { get; set; }
    public HttpClient HttpClientAppCreator { get; set; }
    public AppsClient AppsClient => new(HttpClientAppUser);
    public AppsClient AppsClientCreator => new(HttpClientAppCreator);
    public SecureObjectsClient SecuresObjectClient => new(HttpClientAppUser);
    public RolesClient RolesClient => new(HttpClientAppUser);
    public AuthenticationHeaderValue AuthorizationAppCreator { get; private set; } = default!;
    public AuthenticationHeaderValue AuthorizationAppUser { get; private set; } = default!;

    private TestInit(Dictionary<string, string?> appSettings, string environment)
    {
        // Application
        WebApp = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                foreach (var appSetting in appSettings)
                    builder.UseSetting(appSetting.Key, appSetting.Value);

                builder.UseEnvironment(environment);

                builder.ConfigureServices(_ =>
                {
                });
            });

        // Client
        HttpClientAppUser = WebApp.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        // Client AppCreator
        HttpClientAppCreator = WebApp.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        Scope = WebApp.Services.CreateScope();
    }

    public static async Task<TestInit> Create(Dictionary<string, string?>? appSettings = null, string environment = "Development")
    {
        appSettings ??= new Dictionary<string, string?>();
        var testInit = new TestInit(appSettings, environment);
        await testInit.Init();
        return testInit;
    }

    private async Task Init()
    {
        // build appCreator
        var tokenBuilder = Scope.ServiceProvider.GetRequiredService<BotAuthenticationTokenBuilder>();
        AuthorizationAppCreator = await tokenBuilder.CreateAuthenticationHeader(AppSettings.AppCreatorEmail, AppSettings.AppCreatorEmail);
        HttpClientAppCreator.DefaultRequestHeaders.Authorization = AuthorizationAppCreator;

        // create app
        var appsClient = new AppsClient(HttpClientAppCreator);
        var app = await appsClient.CreateAsync();
        AppId = app.AppId;

        // attach its token
        var token = await AppsClientCreator.GetAuthorizationTokenAsync(AppId);
        AuthorizationAppUser = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
        HttpClientAppUser.DefaultRequestHeaders.Authorization = AuthorizationAppUser;
    }
}