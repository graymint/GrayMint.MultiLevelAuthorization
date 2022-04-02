using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Server;
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test;

public abstract class BaseControllerTest
{
    protected WebApplicationFactory<Program> WebApplication { get; private set; } = default!;
    protected HttpClient HttpClient { get; private set; } = default!;
    protected AuthenticationHeaderValue AppCreatorAuthorization { get; private set; } = default!;
    protected string AppId { get; private set; } = default!;

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
        var key = WebApplication.Services.GetRequiredService<IOptions<AppOptions>>().Value.AuthenticationKey;
        AppCreatorAuthorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, Program.CreateAppCreatorToken(key));
        HttpClient.DefaultRequestHeaders.Authorization = AppCreatorAuthorization;

        var controller = new AppController(HttpClient);
        AppId = await controller.AppsAsync(new AppCreateRequest
        {
            AppName = $"test_{Guid.NewGuid().ToString()}",
            AppDescription = "test application"
        }); 
        var appToken = await controller.AuthenticationTokenAsync(AppId);

        // Init HttpClient for the created app
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, appToken);

    }
}