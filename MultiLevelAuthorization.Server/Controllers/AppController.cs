using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Server.DTOs;
using MultiLevelAuthorization.Server.Models;
using App = MultiLevelAuthorization.Server.Models.App;

namespace MultiLevelAuthorization.Server.Controllers;

[ApiController]
[Route("/api/apps")]
public class AppController : ControllerBase
{
    private readonly Application _application;
    private readonly ApplicationDbContext _dbContext;

    public AppController(Application application, ApplicationDbContext dbContext)
    {
        _application = application;
        _dbContext = dbContext;
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "Robot", Roles = "AppCreator")]
    public async Task<App> Create(AppCreateRequest request)
    {
        var app = await _dbContext.Apps.AddAsync(new App
        {
            AppId = Guid.NewGuid(),
            AppName = request.AppName
        });

        // Create 
        await _dbContext.SaveChangesAsync();
        return app.Entity;
    }

    [HttpPost("{appId:guid}/init")]
    public async Task<AppDto> Init(Guid appId, AppInitRequest request)
    {
        //todo: check permission

        var authManager = new AuthManager(_dbContext, appId);
        var ret = await authManager.Init(request.SecureObjectTypes, request.Permissions, request.PermissionGroups, request.RemoveOtherPermissionGroups);
        return ret;
    }

    [HttpGet("{appId:guid}/permission-groups")]
    public async Task<PermissionGroup[]> PermissionGroups(Guid appId)
    {
        //todo: check permission

        var authManager = new AuthManager(_dbContext, appId);
        var ret = await authManager.GetPermissionGroups();
        return ret;
    }

    [HttpGet("{appId:guid}/authentication-token")]
    [Produces(MediaTypeNames.Text.Plain)]
    [Authorize(AuthenticationSchemes = "Robot", Roles = "AppCreator")]
    public async Task<string> GetAuthenticationToken(Guid appId)
    {
        //todo: check permission

        var app = await _dbContext.Apps.SingleAsync(x => x.AppId == appId);
        var jwt = JwtTool.CreateSymmetricJwt(_application.AuthenticationKey, Application.Issuer, Application.Issuer,
            app.AppId.ToString(), null, new[] { "AppUser" });
        return jwt;
    }
}