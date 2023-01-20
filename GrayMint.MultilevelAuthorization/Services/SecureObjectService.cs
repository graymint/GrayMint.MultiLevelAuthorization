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

    public async Task<SecureObject> Create(int appId, string secureObjectTypeId, string secureObjectId, string? parentSecureObjectId)
    {
        var dbSecureObjectTypeId = await _authRepo.GetSecureObjectTypeIdByExternalId(appId, secureObjectTypeId);

        // Try to get parentSecureObject
        if (string.IsNullOrEmpty(parentSecureObjectId))
            parentSecureObjectId = await _authRepo.GetSystemSecureObjectExternalId(appId, dbSecureObjectTypeId);

        var parentSecureObject = await GetSecureObjectByExternalId(appId, secureObjectTypeId, parentSecureObjectId);

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

        var secureObject = new SecureObject
        {
            SecureObjectId = secureObjectId,
            SecureObjectTypeId = secureObjectTypeId,
            ParentSecureObjectId = parentSecureObjectId
        };

        return secureObject;
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

    public async Task BuildSystemSecureObject(int appId, int systemSecureObjectTypeId)
    {
        await _authRepo.SaveChangesAsync();

        var secureObject = new SecureObjectModel
        {
            AppId = appId,
            SecureObjectExternalId = SystemSecureObjectId,
            SecureObjectTypeId = systemSecureObjectTypeId,
            ParentSecureObjectId = null
        };

        await _authRepo.AddEntity(secureObject);
        await _authRepo.SaveChangesAsync();
    }

    public async Task<List<SecureObjectTypeModel>> UpdateSecureObjectTypes(int appId, SecureObjectType[] obValues)
    {
        var list = new List<SecureObjectTypeModel>();
        // Get SecureObjectTypes from db
        var dbValues = await _authRepo.GetSecureObjectTypes(appId);

        // add
        foreach (var obValue in obValues.Where(x =>
                     dbValues.All(c => c.AppId == appId && x.SecureObjectTypeId != c.SecureObjectTypeExternalId)))
        {
            var secureObjectModel = new SecureObjectTypeModel
            {
                AppId = appId,
                SecureObjectTypeExternalId = obValue.SecureObjectTypeId
            };

            // create new SecureObjectType
            await _authRepo.AddEntity(secureObjectModel);
            list.Add(secureObjectModel);
        }

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.AppId == appId && x.SecureObjectTypeExternalId != c.SecureObjectTypeId)))
            _authRepo.RemoveEntity(dbValue);

        return list;
    }
}