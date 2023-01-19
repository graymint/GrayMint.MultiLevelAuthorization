using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Services;

namespace MultiLevelAuthorization.Server.Controllers;

[Authorize(SimpleRoleAuth.Policy)]
[ApiVersion("1")]
[ApiController]
[Route("/api/v{version:apiVersion}/apps/{appId:int}/users")]
public class UsersController : Controller
{
    private readonly UserService _userService;
    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("{userId:guid}/roles")]
    public async Task<Role[]> GetUserRoles(int appId, Guid userId)
    {
        var roles = await _userService.GetUserRoles(appId, userId);
        return roles;
    }
}