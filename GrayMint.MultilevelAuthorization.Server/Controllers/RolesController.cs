using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Services;

namespace MultiLevelAuthorization.Server.Controllers;

[Authorize(SimpleRoleAuth.Policy)]
[ApiVersion("1")]
[ApiController]
[Route("/api/v{version:apiVersion}/apps/{appId}/roles")]
public class RolesController : Controller
{
    private readonly RoleService _roleService;

    public RolesController(RoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpPost("role")]
    public async Task<Role> Create(int appId, string roleName, Guid ownerId, Guid modifiedByUserId)
    {
        var result = await _roleService.Create(appId, roleName, ownerId, modifiedByUserId);
        return result;
    }

    [HttpPost("{roleId}/add-user")]
    public async Task<IActionResult> Role_AddUser(int appId, Guid roleId, Guid userId, Guid modifiedByUserId)
    {
        await _roleService.AddUserToRole(appId, roleId, userId, modifiedByUserId);
        return Ok();
    }

    [HttpGet]
    public async Task<Role[]> GetRoles(int appId)
    {
        var roles = await _roleService.GetRoles(appId);
        return roles;
    }

    [HttpGet("users")]
    public async Task<User[]> GetRoleUsers(int appId, Guid roleId)
    {
        var result = await _roleService.GetRoleUsers(appId, roleId);
        return result;
    }
}
