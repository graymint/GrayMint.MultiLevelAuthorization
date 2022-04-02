using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Repositories;
using MultiLevelAuthorization.Server.DTOs;
using System.Net.Mime;
using AppDto = MultiLevelAuthorization.Server.DTOs.AppDto;

namespace MultiLevelAuthorization.Server.Controllers;

[ApiController]
[Route("/api/apps")]
public class AppController : ControllerBase
{
    private readonly AuthManager _authManager;
    private readonly IOptions<AppOptions> _appOptions;
    private readonly IMapper _mapper;

    public AppController(AuthManager authManager, IOptions<AppOptions> appOptions, IMapper mapper)
    {
        _authManager = authManager;
        _appOptions = appOptions;
        _mapper = mapper;
    }

    [HttpPost]
    [Produces(MediaTypeNames.Text.Plain)]
    [Authorize(AuthenticationSchemes = "Robot", Roles = "AppCreator")]
    public async Task<string> Create(AppCreateRequest request)
    {
        var createRequestHandler = _mapper.Map<AppCreateRequestHandler>(request);

        var result = await _authManager.App_Create(createRequestHandler);
        return result;
    }

    [HttpPost("{appId}/init")]
    public async Task<AppDto> Init(string appId, AppInitRequest request)
    {
        var app = await _authManager.App_PropsByName(appId);

        //todo: check permission

        var result = await _authManager.Init(app.AppId, request.SecureObjectTypes, request.Permissions, request.PermissionGroups, request.RemoveOtherPermissionGroups);
        var ret = new AppDto(appId, app.AppName, result.SystemSecureObjectId);
        return ret;
    }

    [HttpGet("{appId}/SecureObjectTypes")]
    public async Task<List<SecureObjectTypeDto>> GetSecureObjectTypes(string appId)
    {
        var app = await _authManager.App_PropsByName(appId);

        //todo: check permission

        var result = await _authManager.GetSecureObjectTypes(app.AppId);
        List<SecureObjectTypeDto> list = new List<SecureObjectTypeDto>();
        foreach (var item in result)
        {
            list.Add(new SecureObjectTypeDto(item.SecureObjectTypeId, item.SecureObjectTypeName));
        }
        return list;
    }

    [HttpGet("{appId}/permission-groups")]
    public async Task<PermissionGroupDto[]> PermissionGroups(string appId)
    {
        var app = await _authManager.App_PropsByName(appId);
        //todo: check permission

        var res = await _authManager.GetPermissionGroups(app.AppId);
        var ret = res.Select(
            x => new PermissionGroupDto(
                x.PermissionGroupId,
                x.PermissionGroupName,
                x.Permissions.Select(y => new PermissionDto(y.PermissionId, y.PermissionName)).ToArray())
            );
        return ret.ToArray();
    }

    [HttpGet("{appId}/authentication-token")]
    [Produces(MediaTypeNames.Text.Plain)]
    [Authorize(AuthenticationSchemes = "Robot", Roles = "AppCreator")]
    public async Task<string> GetAuthenticationToken(string appId)
    {
        //todo: check permission

        var app = await _authManager.App_PropsByName(appId);
        var jwt = JwtTool.CreateSymmetricJwt(_appOptions.Value.AuthenticationKey, AppOptions.AuthIssuer, AppOptions.AuthIssuer,
            app.AppId.ToString(), null, new[] { "AppUser" });
        return jwt;
    }
}