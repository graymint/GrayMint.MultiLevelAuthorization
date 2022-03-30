using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Server.DTOs;
using MultiLevelAuthorization.Server.Models;
using App = MultiLevelAuthorization.Server.Models.App;
using AppDto = MultiLevelAuthorization.Server.DTOs.AppDto;

namespace MultiLevelAuthorization.Server.Controllers;

[ApiController]
[Route("/api/apps")]
public class AppController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AuthDbContext _authDbContext;
    private readonly IOptions<AppOptions> _appOptions;

    public AppController(ApplicationDbContext dbContext, AuthDbContext authDbContext, IOptions<AppOptions> appOptions)
    {
        _dbContext = dbContext;
        _authDbContext = authDbContext;
        _appOptions = appOptions;
    }

    [HttpPost]
    [Authorize(AuthenticationSchemes = "Robot", Roles = "AppCreator")]
    public async Task<Guid> Create(AppCreateRequest request)
    {
        // Create dbo.App
        var appEntity = (await _dbContext.Apps.AddAsync(new App()
        {
            AppName = request.AppName,
            AppGuid = Guid.NewGuid()
        })).Entity;
        await _dbContext.SaveChangesAsync();

        // Create auth.App
        var appEntity2 = (await _authDbContext.Apps.AddAsync(new MultiLevelAuthorization.Models.App()
        {
            AppId = appEntity.AppId
        })).Entity;
        await _authDbContext.SaveChangesAsync();

        return appEntity.AppGuid;
    }

    [HttpPost("{appId}/init")]
    public async Task<AppDto> Init(Guid appId, AppInitRequest request)
    {
        var app = await _dbContext.Apps.SingleAsync(x => x.AppGuid == appId);

        //todo: check permission

        var authManager = new AuthManager(_authDbContext, app.AppId);
        var result = await authManager.Init(request.SecureObjectTypes, request.Permissions, request.PermissionGroups, request.RemoveOtherPermissionGroups);
        var ret = new AppDto(appId, app.AppName, result.SystemSecureObjectId);
        return ret;
    }

    [HttpGet("{appId}/permission-groups")]
    public async Task<PermissionGroupDto[]> PermissionGroups(Guid appId)
    {
        var app = await _dbContext.Apps.SingleAsync(x => x.AppGuid == appId);
        //todo: check permission

        var authManager = new AuthManager(_authDbContext, app.AppId);
        var res = await authManager.GetPermissionGroups();
        var ret = res.Select(
            x => new PermissionGroupDto(
                x.PermissionGroupId,
                x.PermissionGroupName,
                x.Permissions.Select(y=>new PermissionDto(y.PermissionId, y.PermissionName)).ToArray())
            );
        return ret.ToArray();
    }

    [HttpGet("{appId}/authentication-token")]
    [Produces(MediaTypeNames.Text.Plain)]
    [Authorize(AuthenticationSchemes = "Robot", Roles = "AppCreator")]
    public async Task<string> GetAuthenticationToken(Guid appId)
    {
        //todo: check permission

        var app = await _dbContext.Apps.SingleAsync(x => x.AppGuid == appId);
        var jwt = JwtTool.CreateSymmetricJwt(_appOptions.Value.AuthenticationKey, AppOptions.AuthIssuer, AppOptions.AuthIssuer,
            app.AppId.ToString(), null, new[] { "AppUser" });
        return jwt;
    }
}