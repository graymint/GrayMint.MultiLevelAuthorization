using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Services;

namespace MultiLevelAuthorization.Server.Controllers;


[Authorize(SimpleRoleAuth.Policy)]
[ApiVersion("1")]
[ApiController]
[Route("/api/v{version:apiVersion}/apps/{appId:int}/secure-objects")]

public class SecureObjectsController : Controller
{
    private readonly SecureObjectService _secureObjectService;

    public SecureObjectsController(SecureObjectService secureObjectService)
    {
        _secureObjectService = secureObjectService;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost]
    public async Task<SecureObject> Create(int appId, string secureObjectTypeId, string secureObjectId, string parentSecureObjectTypeId, string parentSecureObjectId)
    {
        var result = await _secureObjectService.Create(appId, secureObjectTypeId, secureObjectId, parentSecureObjectTypeId, parentSecureObjectId);
        return result;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("{secureObjectTypeId}/{secureObjectId}/add-user-permission")]
    public async Task<IActionResult> AddUserPermission(int appId, string secureObjectTypeId, string secureObjectId, Guid userId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        await _secureObjectService.AddUserPermission(appId, secureObjectTypeId, secureObjectId, userId, permissionGroupId, modifiedByUserId);
        return Ok();
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("{secureObjectTypeId}/{secureObjectId}/add-role-permission")]
    public async Task<IActionResult> AddRolePermission(int appId, string secureObjectTypeId, string secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        await _secureObjectService.AddRolePermission(appId, secureObjectTypeId, secureObjectId, roleId, permissionGroupId, modifiedByUserId);
        return Ok();
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("{secureObjectTypeId}/{secureObjectId}/move")]
    public async Task<SecureObject> Move(int appId, string secureObjectTypeId, string secureObjectId,
        string prentSecureObjectTypeId, string parentSecureObjectId)
    {
        var secureObject = await _secureObjectService.Move(appId, secureObjectTypeId, secureObjectId,
            prentSecureObjectTypeId, parentSecureObjectId);
        return secureObject;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("{secureObjectTypeId}/{secureObjectId}/user-permissions")]
    public async Task<Permission[]> GetUserPermissions(int appId, string secureObjectTypeId, string secureObjectId, Guid userId)
    {
        var userPermissions = await _secureObjectService.GetUserPermissions(appId, secureObjectTypeId, secureObjectId, userId);
        return userPermissions;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("{secureObjectTypeId}/{secureObjectId}/has-user-permission")]
    public async Task<bool> HasUserPermission(int appId, string secureObjectTypeId, string secureObjectId, Guid userId, int permissionId)
    {
        var hasPermission = await _secureObjectService.HasUserPermission(appId, secureObjectTypeId, secureObjectId, userId, permissionId);
        return hasPermission;
    }
}