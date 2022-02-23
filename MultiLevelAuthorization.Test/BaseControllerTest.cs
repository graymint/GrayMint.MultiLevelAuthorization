using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Server;
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test;

public abstract class BaseControllerTest
{
    protected WebApplicationFactory<Program> WebApplication { get; private set; } = default!;
    protected HttpClient HttpClient { get; private set; } = default!;
    protected AuthenticationHeaderValue AppCreatorAuthorization { get; private set; } = default!;
    protected App App { get; private set; } = default!;

    [TestInitialize]
    public virtual async Task Init()
    {
        // Application
        WebApplication = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(_ =>
                {
                });
            });

        // Client
        HttpClient = WebApplication.CreateClient();
        HttpClient.BaseAddress = new Uri("https://localhost/");

        // Create New App
        var app = WebApplication.Services.GetRequiredService<Application>();
        AppCreatorAuthorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, app.CreateAppCreatorToken());
        HttpClient.DefaultRequestHeaders.Authorization = AppCreatorAuthorization;
        
        var controller = new AppController(HttpClient);
        App = await controller.AppsAsync(new AppCreateRequest
        {
            AppName = $"test_{Guid.NewGuid()}"
        });
        var appToken = await controller.AuthenticationTokenAsync(App.AppId);

        // Init HttpClient for the created app
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, appToken);
        
    }
}