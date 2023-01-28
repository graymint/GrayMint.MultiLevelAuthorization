using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Services;

namespace MultiLevelAuthorization.Server.Controllers;

[Authorize(SimpleRoleAuth.Policy)]
[ApiVersion("1")]
[ApiController]
[Route("/api/v{version:apiVersion}/apps/{appId:int}")]
public class RolesController : Controller
{
    private readonly RoleService _roleService;
    private readonly SecureObjectService _secureObjectService;

    public RolesController(RoleService roleService, SecureObjectService secureObjectService)
    {
        _roleService = roleService;
        _secureObjectService = secureObjectService;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("secure-object-types/{secureObjectTypeId}/secure-objects/{secureObjectId}/roles")]
    public async Task<Role> Create(int appId, string roleName, string secureObjectTypeId, string secureObjectId, Guid modifiedByUserId)
    {
        var result = await _roleService.Create(appId, roleName, secureObjectTypeId, secureObjectId, modifiedByUserId);
        return result;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("secure-object-types/{secureObjectTypeId}/secure-objects/{secureObjectId}/roles")]
    public async Task<Role[]> GetRoles(int appId, string secureObjectTypeId, string secureObjectId)
    {
        var dbSecureObject = await _secureObjectService.GetSecureObjectByExternalId(appId, secureObjectTypeId, secureObjectId);
        var roles = await _roleService.GetRoles(appId, dbSecureObject.SecureObjectId);
        return roles;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("roles/{roleId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> AddUser(int appId, Guid roleId, Guid userId, Guid modifiedByUserId)
    {
        await _roleService.AddUserToRole(appId, roleId, userId, modifiedByUserId);
        return Ok();
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("roles/{roleId}")]
    public async Task<Role> GetRole(int appId, Guid roleId)
    {
        var result = await _roleService.GetRole(appId, roleId);
        return result;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("users/{userId:guid}/roles")]
    public async Task<Role[]> GetUserRoles(int appId, Guid userId)
    {
        var roles = await _roleService.GetUserRoles(appId, userId);
        return roles;
    }
}
