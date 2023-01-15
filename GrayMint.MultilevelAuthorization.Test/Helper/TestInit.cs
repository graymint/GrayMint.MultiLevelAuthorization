using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GrayMint.Common.AspNetCore.Auth.BotAuthentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MultiLevelAuthorization.Server;
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test.Helper;

public class TestInit
{
    public IServiceScope Scope { get; }
    public int AppId { get; private set; }
    public AppSettings AppSettings => WebApp.Services.GetRequiredService<IOptions<AppSettings>>().Value;
    public WebApplicationFactory<Program> WebApp { get; set; }
    public HttpClient HttpClientAppUser { get; set; }
    public HttpClient HttpClientAppCreator { get; set; }
    protected AuthenticationHeaderValue AppCreatorAuthorization { get; private set; } = default!;
    public AppsClient AppsClient => new(HttpClientAppCreator);

    private TestInit ()
    {
        // Application
        WebApp = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
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

    public static async Task<TestInit> Create()
    {
        var testInit = new TestInit();
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
        AppId = (await appsClient.CreateAsync(new AppCreateRequest { AppName = Guid.NewGuid().ToString() })).AppId;

        // attach its token
        var token = await AppsClient.GetAuthorizationTokenAsync(AppId);
        AuthorizationAppUser = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
        HttpClientAppUser.DefaultRequestHeaders.Authorization = AuthorizationAppUser;
    }

}