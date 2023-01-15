using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Services;

namespace MultiLevelAuthorization.Server.Controllers;

[Authorize(SimpleRoleAuth.Policy)]
[ApiVersion("1")]
[ApiController]
[Route("/api/v{version:apiVersion}/apps/{appId}")]
public class PermissionController : Controller
{
    private readonly PermissionService _permissionService;
    public PermissionController(PermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet("permission-groups")]
    public async Task<PermissionGroup[]> GetPermissionGroups(int appId)
    {
        var permissionGroups = await _permissionService.GetPermissionGroups(appId);
        return permissionGroups;
    }
}