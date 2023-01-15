using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Services;

namespace MultiLevelAuthorization.Server.Controllers;


[Authorize(SimpleRoleAuth.Policy)]
[ApiVersion("1")]
[ApiController]
[Route("/api/v{version:apiVersion}/apps/{appId}/secure-objects")]

public class SecureObjectsController : Controller
{
    private readonly SecureObjectService _secureObjectService;

    public SecureObjectsController(SecureObjectService secureObjectService)
    {
        _secureObjectService = secureObjectService;
    }

    [HttpGet("secure-object-types")]
    public async Task<SecureObjectType[]> GetSecureObjectTypes(int appId)
    {
        var secureObjectTypes = await _secureObjectService.GetSecureObjectTypes(appId);
        return secureObjectTypes;
    }

    [HttpGet("secure-objects")]
    public async Task<SecureObject[]> GetSecureObjects(int appId)
    {
        var secureObjects = await _secureObjectService.GetSecureObjects(appId);
        return secureObjects;
    }

    [HttpPost]
    public async Task<SecureObject> Create(int appId, Guid secureObjectId, Guid secureObjectTypeId, Guid? parentSecureObjectId)
    {
        var result = await _secureObjectService.Create(appId, secureObjectId, secureObjectTypeId, parentSecureObjectId);
        return result;
    }

    [HttpPost("{secureObjectId}/add-user-permission")]
    public async Task<IActionResult> AddUserPermission(int appId, Guid secureObjectId, Guid userId, Guid permissionGroupId,
        Guid modifiedByUserId)
    {
        await _secureObjectService.AddUserPermission(appId, secureObjectId, userId, permissionGroupId, modifiedByUserId);
        return Ok();
    }

    [HttpPost("{secureObjectId}/add-role-permission")]
    public async Task<IActionResult> AddRolePermission(int appId, Guid secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        await _secureObjectService.AddRolePermission(appId, secureObjectId, roleId, permissionGroupId, modifiedByUserId);
        return Ok();
    }

    [HttpGet("{secureObjectId}/user-permissions")]
    public async Task<Permission[]> GetUserPermissions(int appId, Guid secureObjectId, Guid userId)
    {
        var userPermissions = await _secureObjectService.GetUserPermissions(appId, secureObjectId, userId);
        return userPermissions;
    }
}