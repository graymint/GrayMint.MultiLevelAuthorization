using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiLevelAuthorization.Server.Models;

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
    //public async Task<AppDto> Create(AppCreateRequest request)
    //{
    //    //var appEntity = (await _dbContext.Apps.AddAsync(new App
    //    //{
    //    //    AppId = Guid.NewGuid(),
    //    //    AppName = request.AppName
    //    //})).Entity;

    //    //// Create 
    //    //await _dbContext.SaveChangesAsync();
    //    //var app = new AppDto(appEntity.AppId, appEntity.);
    //    //return app;
    //    throw new NotImplementedException();
    //}

    [HttpPost("{appId}/init")]
    //public async Task<AppDto> Init(Guid appId, AppInitRequest request)
    //{
    //    //todo: check permission

    //    var authManager = new AuthManager(_dbContext, appId);
    //    var ret = await authManager.Init(request.SecureObjectTypes, request.Permissions, request.PermissionGroups, request.RemoveOtherPermissionGroups);
    //    return ret;
    //}

    [HttpGet("{appId}/permission-groups")]
    //public async Task<PermissionGroup[]> PermissionGroups(Guid appId)
    //{
    //    //todo: check permission

    //    var authManager = new AuthManager(_dbContext, appId);
    //    var ret = await authManager.GetPermissionGroups();
    //    return ret;
    //}

    [HttpGet("{appId}/authentication-token")]
    [Produces(MediaTypeNames.Text.Plain)]
    [Authorize(AuthenticationSchemes = "Robot", Roles = "AppCreator")]
    public async Task<string> GetAuthenticationToken(int appId)
    {
        //todo: check permission

        var app = await _dbContext.Apps.SingleAsync(x => x.AppId == appId);
        var jwt = JwtTool.CreateSymmetricJwt(_application.AuthenticationKey, Application.Issuer, Application.Issuer,
            app.AppId.ToString(), null, new[] { "AppUser" });
        return jwt;
    }
}