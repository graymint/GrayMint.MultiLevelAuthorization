using Microsoft.AspNetCore.Mvc;
using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Server.Controllers;

[ApiController]
[Route("/api/apps")]
public class PermissionController : Controller
{
    private readonly AuthManager _authManager;

    public PermissionController(AuthManager authManager)
    {
        _authManager = authManager;
    }

    [HttpGet("{appId}/permission-groups")]
    public async Task<PermissionGroupDto[]> PermissionGroup_List(string appId)
    {
        //todo: check permission

        var res = await _authManager.PermissionGroup_List(appId);
        var ret = res.Select(
            x => new PermissionGroupDto(
                x.PermissionGroupExternalId,
                x.PermissionGroupName,
                x.Permissions.Select(y => new PermissionDto(y.PermissionId, y.PermissionName)).ToArray())
        );
        return ret.ToArray();
    }
}