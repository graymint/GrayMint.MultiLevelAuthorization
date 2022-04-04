using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Repositories;
using MultiLevelAuthorization.Server.DTOs;
using System.Net.Mime;
using AppDto = MultiLevelAuthorization.Server.DTOs.AppDto;

namespace MultiLevelAuthorization.Server.Controllers;

[ApiController]
[Route("/api/apps")]

public class AuthorizationController : ControllerBase
{
    private readonly AuthManager _authManager;
    private readonly IOptions<AppOptions> _appOptions;
    private readonly IMapper _mapper;

    public AuthorizationController(AuthManager authManager, IOptions<AppOptions> appOptions, IMapper mapper)
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
    public async Task<List<SecureObjectTypeDto>> SecureObjectType_List(string appId)
    {
        var app = await _authManager.App_PropsByName(appId);

        //todo: check permission

        var result = await _authManager.SecureObjectType_List(app.AppId);
        List<SecureObjectTypeDto> list = new List<SecureObjectTypeDto>();
        foreach (var item in result)
        {
            list.Add(new SecureObjectTypeDto(item.SecureObjectTypeId, item.SecureObjectTypeName));
        }
        return list;
    }

    [HttpGet("{appId}/SecureObjects")]
    public async Task<List<SecureObjectDto>> SecureObject_List(string appId)
    {
        var app = await _authManager.App_PropsByName(appId);

        //todo: check permission

        var result = await _authManager.SecureObject_List(app.AppId);
        List<SecureObjectDto> list = new List<SecureObjectDto>();
        foreach (var item in result)
        {
            list.Add(new SecureObjectDto(item.SecureObjectId, item.SecureObjectTypeId, item.ParentSecureObjectId));
        }
        return list;
    }

    [HttpGet("{appId}/permission-groups")]
    public async Task<PermissionGroupDto[]> PermissionGroup_List(string appId)
    {
        var app = await _authManager.App_PropsByName(appId);
        //todo: check permission

        var res = await _authManager.PermissionGroup_List(app.AppId);
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

    [HttpPost("{appId}/SecureObject")]
    public async Task<SecureObjectDto> SecureObject_Create(string appId, Guid secureObjectId, Guid secureObjectTypeId, Guid? parentSecureObjectId)
    {
        var app = await _authManager.App_PropsByName(appId);

        //todo: check permission

        var result = await _authManager.SecureObject_Create(app.AppId, secureObjectId, secureObjectTypeId, parentSecureObjectId);
        return result;
    }

    [HttpPost("{appId}/Role")]
    public async Task<Role> Role_Create(string appId, string roleName, Guid ownerId, Guid modifiedByUserId)
    {
        var app = await _authManager.App_PropsByName(appId);

        //todo: check permission

        var result = await _authManager.Role_Create(app.AppId, roleName, ownerId, modifiedByUserId);
        return result;
    }

    [HttpGet("{appId}/Roles")]
    public async Task<Role[]> Role_List(string appId)
    {
        //todo: check permission

        var app = await _authManager.App_PropsByName(appId);
        var result = await _authManager.Role_List(app.AppId);

        return result;
    }
    [HttpGet("{appId}/role-users")]
    public async Task<RoleUser[]> Role_Users(string appId, Guid roleId)
    {
        //todo: check permission

        var app = await _authManager.App_PropsByName(appId);
        var result = await _authManager.Role_Users(app.AppId, roleId);

        return result;
    }

}