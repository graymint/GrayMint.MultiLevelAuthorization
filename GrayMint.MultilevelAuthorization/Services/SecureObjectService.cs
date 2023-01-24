using System.Security;
using MultiLevelAuthorization.DtoConverters;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Services;

public class SecureObjectService
{
    public const string SystemSecureObjectId = "$";
    public const string SystemSecureObjectTypeId = "$";

    private readonly AuthRepo _authRepo;
    private readonly PermissionService _permissionService;

    public SecureObjectService(
        AuthRepo authRepo,
        PermissionService permissionService)
    {
        _authRepo = authRepo;
        _permissionService = permissionService;
    }

    public async Task<SecureObject> Create(int appId, string secureObjectTypeId, string secureObjectId, string? parentSecureObjectTypeId, string? parentSecureObjectId)
    {
        var dbSecureObjectTypeId = await _authRepo.GetSecureObjectTypeIdByExternalId(appId, secureObjectTypeId);

        // Try to get parentSecureObject
        if (string.IsNullOrWhiteSpace(parentSecureObjectTypeId))
            parentSecureObjectTypeId = SystemSecureObjectTypeId;

        if (string.IsNullOrWhiteSpace(parentSecureObjectId))
            parentSecureObjectId = SystemSecureObjectId;

        var parentSecureObject = await GetSecureObjectByExternalId(appId, parentSecureObjectTypeId, parentSecureObjectId);

        // Prepare SecureObject
        var secureObjectModel = new SecureObjectModel
        {
            AppId = appId,
            SecureObjectExternalId = secureObjectId,
            SecureObjectTypeId = dbSecureObjectTypeId,
            ParentSecureObjectId = parentSecureObject.SecureObjectId
        };
        await _authRepo.AddEntity(secureObjectModel);
        await _authRepo.SaveChangesAsync();

        return secureObjectModel.ToDto();
    }

    public async Task<SecureObjectModel> GetSecureObjectByExternalId(int appId, string secureObjectTypeId, string secureObjectId)
    {
        // prevent empty or null values, because it may be create a Null parent node.
        ArgumentException.ThrowIfNullOrEmpty(secureObjectId);
        ArgumentException.ThrowIfNullOrEmpty(secureObjectTypeId);

        var secureObject = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectTypeId, secureObjectId);
        return secureObject;
    }

    public async Task AddRolePermission(int appId, string secureObjectTypeId, string secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        // try to get db values for Ids
        var dbPermissionGroupId = await _permissionService.GetPermissionGroupIdByExternalId(appId, permissionGroupId);
        var secureObject = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectTypeId, secureObjectId);

        var secureObjectRolePermission = new SecureObjectRolePermissionModel
        {
            AppId = appId,
            SecureObjectId = secureObject.SecureObjectId,
            RoleId = roleId,
            PermissionGroupId = dbPermissionGroupId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
        };

        await _authRepo.AddEntity(secureObjectRolePermission);
        await _authRepo.SaveChangesAsync();
    }

    public async Task<SecureObject> Update(int appId, string secureObjectTypeId, string secureObjectId, SecureObjectUpdateRequest request)
    {
        // Get AppModel
        var secureObject = await Move(appId, secureObjectTypeId, secureObjectId, request.ParentSecureObjectTypeId, request.ParentSecureObjectId);
        return secureObject;
    }
    private async Task<SecureObject> Move(int appId, string secureObjectTypeId, string secureObjectId,
        string parentSecureObjectTypeId, string parentSecureObjectId)
    {
        // get secure objects info
        var secureObject = await GetSecureObjectByExternalId(appId, secureObjectTypeId, secureObjectId);
        var parentSecureObject = await GetSecureObjectByExternalId(appId, parentSecureObjectTypeId, parentSecureObjectId);

        if (secureObject.SecureObjectId == parentSecureObject.SecureObjectId)
            throw new InvalidOperationException("SecureObject and ParentSecureObject can not be same.");

        secureObject.ParentSecureObjectId = parentSecureObject.SecureObjectId;
        await _authRepo.SaveChangesAsync();

        var secureObjectModel = await GetSecureObjectByExternalId(appId, secureObjectTypeId, secureObjectId);
        return secureObjectModel.ToDto();
    }

    public async Task AddUserPermission(int appId, string secureObjectTypeId, string secureObjectId, Guid userId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        // try to get db values for Ids
        var dbPermissionGroupId = await _permissionService.GetPermissionGroupIdByExternalId(appId, permissionGroupId);
        var secureObject = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectTypeId, secureObjectId);

        var secureObjectUserPermission = new SecureObjectUserPermissionModel
        {
            AppId = appId,
            SecureObjectId = secureObject.SecureObjectId,
            UserId = userId,
            PermissionGroupId = dbPermissionGroupId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
        };
        await _authRepo.AddEntity(secureObjectUserPermission);
        await _authRepo.SaveChangesAsync();
    }

    public async Task<PermissionGroup[]> GetSecureObjectUserPermissions(int appId, string secureObjectTypeId, string secureObjectId, Guid userId)
    {
        var secureObject = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectTypeId, secureObjectId);
        var userPermissionGroupModels = await _authRepo.GetSecureObjectUserPermissions(appId, secureObject.SecureObjectId, userId);
        return userPermissionGroupModels.Select(x => x.ToDto()).ToArray();
    }

    public async Task<PermissionGroup[]> GetSecureObjectRolePermissions(int appId, string secureObjectTypeId, string secureObjectId, Guid roleId)
    {
        var secureObject = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectTypeId, secureObjectId);
        var rolePermissionGroupModels = await _authRepo.GetSecureObjectRolePermissions(appId, secureObject.SecureObjectId, roleId);
        return rolePermissionGroupModels.Select(x => x.ToDto()).ToArray();
    }

    public async Task<bool> HasUserPermission(int appId, string secureObjectTypeId, string secureObjectId, Guid userId, int permissionId)
    {
        // retrieve db model for secureObject
        var secureObject = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectTypeId, secureObjectId);

        var permissions = await _authRepo.GetUserPermissions(appId, secureObject.SecureObjectId, userId);
        return permissions.Any(x => x.PermissionId == permissionId);
    }

    public async Task<Permission[]> GetUserPermissions(int appId, string secureObjectTypeId, string secureObjectId, Guid userId)
    {
        // retrieve db model for secureObject
        var secureObject = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectTypeId, secureObjectId);

        var permissions = await _authRepo.GetUserPermissions(appId, secureObject.SecureObjectId, userId);
        return permissions.Select(x => x.ToDto()).ToArray();
    }

    public async Task VerifyUserPermission(int appId, string secureObjectId, string secureObjectTypeId, Guid userId, int permissionId)
    {
        if (!await HasUserPermission(appId, secureObjectId, secureObjectTypeId, userId, permissionId))
            throw new SecurityException("You need to grant permission!");
    }

    public async Task BuildSystemSecureObject(int appId)
    {
        await _authRepo.SaveChangesAsync();

        var secureObjectType = new SecureObjectTypeModel
        {
            AppId = appId,
            SecureObjectTypeExternalId = SystemSecureObjectTypeId
        };

        await _authRepo.AddEntity(secureObjectType);
        await _authRepo.SaveChangesAsync();


        var secureObject = new SecureObjectModel
        {
            AppId = appId,
            SecureObjectExternalId = SystemSecureObjectId,
            SecureObjectTypeId = secureObjectType.SecureObjectTypeId,
            ParentSecureObjectId = null
        };

        await _authRepo.AddEntity(secureObject);
        await _authRepo.SaveChangesAsync();
    }

    public async Task UpdateSecureObjectTypes(int appId, SecureObjectType[] obValues)
    {
        // Get SecureObjectTypes from db
        var dbValues = await _authRepo.GetSecureObjectTypes(appId);

        // add
        foreach (var obValue in obValues.Where(x =>
                     dbValues.All(c => c.AppId == appId && x.SecureObjectTypeId != c.SecureObjectTypeExternalId)))
            await _authRepo.AddEntity(new SecureObjectTypeModel
            {
                AppId = appId,
                SecureObjectTypeExternalId = obValue.SecureObjectTypeId
            });

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.AppId == appId && x.SecureObjectTypeExternalId != c.SecureObjectTypeId)))
            _authRepo.RemoveEntity(dbValue);
    }
}