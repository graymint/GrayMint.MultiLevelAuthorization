using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Services;

namespace MultiLevelAuthorization.Server.Controllers;

[Authorize(SimpleRoleAuth.Policy)]
[ApiVersion("1")]
[ApiController]
[Route("/api/v{version:apiVersion}/apps/{appId:int}/roles")]
public class RolesController : Controller
{
    private readonly RoleService _roleService;

    public RolesController(RoleService roleService)
    {
        _roleService = roleService;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost]
    public async Task<Role> Create(int appId, string roleName, Guid ownerId, Guid modifiedByUserId)
    {
        var result = await _roleService.Create(appId, roleName, ownerId, modifiedByUserId);
        return result;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("{roleId:guid}/add-user")]
    public async Task<IActionResult> AddUserToRole(int appId, Guid roleId, Guid userId, Guid modifiedByUserId)
    {
        await _roleService.AddUserToRole(appId, roleId, userId, modifiedByUserId);
        return Ok();
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet]
    public async Task<Role[]> GetRoles(int appId)
    {
        var roles = await _roleService.GetRoles(appId);
        return roles;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("users")]
    public async Task<User[]> GetRoleUsers(int appId, Guid roleId)
    {
        var result = await _roleService.GetRoleUsers(appId, roleId);
        return result;
    }
}
