﻿using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
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
    [HttpGet("secure-objects")]
    public async Task<SecureObject[]> GetSecureObjects(int appId)
    {
        var secureObjects = await _secureObjectService.GetSecureObjects(appId);
        return secureObjects;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost]
    public async Task<SecureObject> Create(int appId, Guid secureObjectId, Guid secureObjectTypeId, Guid? parentSecureObjectId)
    {
        var result = await _secureObjectService.Create(appId, secureObjectId, secureObjectTypeId, parentSecureObjectId);
        return result;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("{secureObjectId:guid}/add-user-permission")]
    public async Task<IActionResult> AddUserPermission(int appId, Guid secureObjectId, Guid userId, Guid permissionGroupId,
        Guid modifiedByUserId)
    {
        await _secureObjectService.AddUserPermission(appId, secureObjectId, userId, permissionGroupId, modifiedByUserId);
        return Ok();
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpPost("{secureObjectId:guid}/add-role-permission")]
    public async Task<IActionResult> AddRolePermission(int appId, Guid secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        await _secureObjectService.AddRolePermission(appId, secureObjectId, roleId, permissionGroupId, modifiedByUserId);
        return Ok();
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("{secureObjectId:guid}/user-permissions")]
    public async Task<Permission[]> GetUserPermissions(int appId, Guid secureObjectId, Guid userId)
    {
        var userPermissions = await _secureObjectService.GetUserPermissions(appId, secureObjectId, userId);
        return userPermissions;
    }

    [Authorize(SimpleRoleAuth.Policy, Roles = Roles.AppUser)]
    [HttpGet("{secureObjectId:guid}/verify-user-permission")]
    public async Task VerifyUserPermission(int appId, Guid secureObjectId, Guid userId, int permissionId)
    {
        await _secureObjectService.VerifyUserPermission(appId, secureObjectId, userId, permissionId);
    }
}