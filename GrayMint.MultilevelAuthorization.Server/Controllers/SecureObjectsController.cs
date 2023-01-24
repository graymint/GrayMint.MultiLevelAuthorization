using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Services;

namespace MultiLevelAuthorization.Server.Controllers;


[Authorize(SimpleRoleAuth.Policy)]
[ApiVersion("1")]
[ApiController]
[Route("/api/v{version:apiVersion}/apps/{appId:int}/secure-object-types")]

public class SecureObjectsController : Controller
{
    private readonly SecureObjectService _secureObjectService;

    public SecureObjectsController(SecureObjectService secureObjectService)
    {
        _secureObjectService = secureObjectService;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("{secureObjectTypeId}/secure-objects/{secureObjectId}")]
    public async Task<SecureObject> Create(int appId, string secureObjectTypeId, string secureObjectId, string parentSecureObjectTypeId, string parentSecureObjectId)
    {
        var result = await _secureObjectService.Create(appId, secureObjectTypeId, secureObjectId, parentSecureObjectTypeId, parentSecureObjectId);
        return result;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("{secureObjectTypeId}/secure-objects/{secureObjectId}/user-permission-groups")]
    public async Task<IActionResult> AddUserPermission(int appId, string secureObjectTypeId, string secureObjectId, Guid userId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        await _secureObjectService.AddUserPermission(appId, secureObjectTypeId, secureObjectId, userId, permissionGroupId, modifiedByUserId);
        return Ok();
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("{secureObjectTypeId}/secure-objects/{secureObjectId}/user-permission-groups")]
    public async Task<PermissionGroup[]> GetSecureObjectUserPermissions(int appId, string secureObjectTypeId, string secureObjectId, Guid userId)
    {
        var permissionGroups = await _secureObjectService.GetSecureObjectUserPermissions(appId, secureObjectTypeId, secureObjectId, userId);
        return permissionGroups;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("{secureObjectTypeId}/secure-objects/{secureObjectId}/role-permission-groups")]
    public async Task<IActionResult> AddRolePermission(int appId, string secureObjectTypeId, string secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        await _secureObjectService.AddRolePermission(appId, secureObjectTypeId, secureObjectId, roleId, permissionGroupId, modifiedByUserId);
        return Ok();
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("{secureObjectTypeId}/secure-objects/{secureObjectId}/role-permission-groups")]
    public async Task<PermissionGroup[]> GetSecureObjectRolePermissions(int appId, string secureObjectTypeId, string secureObjectId, Guid roleId)
    {
        var permissionGroups = await _secureObjectService.GetSecureObjectRolePermissions(appId, secureObjectTypeId, secureObjectId, roleId);
        return permissionGroups;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPatch("{secureObjectTypeId}/secure-objects/{secureObjectId}")]
    public async Task<SecureObject> Update(int appId, string secureObjectTypeId, string secureObjectId, SecureObjectUpdateRequest request)
    {
        var secureObject = await _secureObjectService.Update(appId, secureObjectTypeId, secureObjectId, request);
        return secureObject;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("{secureObjectTypeId}/secure-objects/{secureObjectId}/users/{userId}/permissions")]
    public async Task<Permission[]> GetUserPermissions(int appId, string secureObjectTypeId, string secureObjectId, Guid userId)
    {
        var userPermissions = await _secureObjectService.GetUserPermissions(appId, secureObjectTypeId, secureObjectId, userId);
        return userPermissions;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("{secureObjectTypeId}/secure-objects/{secureObjectId}/users/{userId}/permissions/{permissionId}")]
    public async Task<bool> HasUserPermission(int appId, string secureObjectTypeId, string secureObjectId, Guid userId, int permissionId)
    {
        var hasPermission = await _secureObjectService.HasUserPermission(appId, secureObjectTypeId, secureObjectId, userId, permissionId);
        return hasPermission;
    }
}