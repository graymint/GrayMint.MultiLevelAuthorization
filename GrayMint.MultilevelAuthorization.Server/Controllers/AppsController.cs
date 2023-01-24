using System.Net.Mime;
using GrayMint.Common.AspNetCore.Auth.BotAuthentication;
using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Services;

namespace MultiLevelAuthorization.Server.Controllers;

[Authorize(SimpleRoleAuth.Policy)]
[ApiVersion("1")]
[ApiController]
[Route("/api/v{version:apiVersion}/apps")]

public class AppsController : Controller
{
    private readonly AppService _appService;
    private readonly BotAuthenticationTokenBuilder _botAuthenticationTokenBuilder;
    private readonly IHostEnvironment _hostEnvironment;

    public AppsController(
        AppService appService,
        BotAuthenticationTokenBuilder botAuthenticationTokenBuilder, IHostEnvironment hostEnvironment)
    {
        _appService = appService;
        _botAuthenticationTokenBuilder = botAuthenticationTokenBuilder;
        _hostEnvironment = hostEnvironment;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("{appId:int}/init")]
    public async Task<App> Init(int appId, AppInitRequest request)
    {
        var result = await _appService.InitApp(appId, request.SecureObjectTypes, request.Permissions, request.PermissionGroups, request.RemoveOtherPermissionGroups);
        return result;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppCreator)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [HttpPost]
    public async Task<ActionResult<App>> Create()
    {
        var app = await _appService.Create();
        return CreatedAtAction(nameof(Get), new { app.AppId }, app);
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("{appId:int}")]
    public async Task<App> Get(int appId)
    {
        var appDto = await _appService.Get(appId);
        return appDto;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = $"{Roles.AppCreator},{Roles.AppUser}")]
    [HttpGet("{appId:int}/authorization-token")]
    [Produces(MediaTypeNames.Application.Json)]
    public async Task<string> GetAuthorizationToken(int appId)
    {
        var email = $"{appId}@local";

        // This function check app in its implementation by provider
        var token = await _botAuthenticationTokenBuilder.CreateAuthenticationHeader(email, email);

        return token.Parameter ?? throw new Exception("Authorization server can not get token information.");
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = $"{Roles.AppCreator},{Roles.AppUser}")]
    [HttpPost("{appId:int}/reset-authorization-token")]
    public async Task<string> ResetAuthorizationToken(int appId)
    {
        // todo warning: it does not work because the GrayMint check by their Cache and need to read directly from database
        await _appService.ResetAuthorizationCode(appId);
        return await GetAuthorizationToken(appId);
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("{appId}/clear-all")]
    public async Task ClearAll(int appId)
    {
        // check access
        if (_hostEnvironment.IsProduction())
            throw new UnauthorizedAccessException("This operation is not support in production.");

        await _appService.ClearAll(appId);
    }
}