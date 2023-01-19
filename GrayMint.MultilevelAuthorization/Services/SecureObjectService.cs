using System.Security;
using MultiLevelAuthorization.DtoConverters;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Services;

public class SecureObjectService
{
    private readonly AuthRepo _authRepo;
    private readonly PermissionService _permissionService;

    public SecureObjectService(
        AuthRepo authRepo,
        PermissionService permissionService
        )
    {
        _authRepo = authRepo;
        _permissionService = permissionService;
    }

    public async Task<SecureObject> Create(int appId, Guid secureObjectId, Guid secureObjectTypeId, Guid? parentSecureObjectId)
    {
        var dbSecureObjectTypeId = await _authRepo.GetSecureObjectTypeIdByExternalId(appId, secureObjectTypeId);

        // Try to get parentSecureObject
        var parentSecureObject = await GetSecureObjectByExternalId(appId, parentSecureObjectId);

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

    private async Task<SecureObjectModel> GetSecureObjectByExternalId(int appId, Guid? parentSecureObjectId)
    {
        var secureObject = parentSecureObjectId != null
            ? await _authRepo.GetSecureObjectByExternalId(appId, (Guid)parentSecureObjectId)
            : await GetRootSecureObject(appId);

        return secureObject;
    }

    public async Task AddRolePermission(int appId, Guid secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        // try to get db values for Ids
        var dbPermissionGroupId = await _permissionService.GetPermissionGroupIdByExternalId(appId, permissionGroupId);
        var secureObject = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectId);

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

    public async Task AddUserPermission(int appId, Guid secureObjectId, Guid userId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        // try to get db values for Ids
        var dbPermissionGroupId = await _permissionService.GetPermissionGroupIdByExternalId(appId, permissionGroupId);
        var secureObject = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectId);

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

    public async Task<bool> HasUserPermission(int appId, Guid secureObjectId, Guid userId, int permissionId)
    {
        // retrieve db model for secureObject
        var secureObject = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectId);

        var permissions = await _authRepo.GetUserPermissions(appId, secureObject.SecureObjectId, userId);
        return permissions.Any(x => x.PermissionId == permissionId);
    }

    public async Task<Permission[]> GetUserPermissions(int appId, Guid secureObjectId, Guid userId)
    {
        // retrieve db model for secureObject
        var secureObject = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectId);

        var permissions = await _authRepo.GetUserPermissions(appId, secureObject.SecureObjectId, userId);
        return permissions.Select(x => x.ToDto()).ToArray();
    }

    public async Task VerifyUserPermission(int appId, Guid secureObjectId, Guid userId, int permissionId)
    {
        if (!await HasUserPermission(appId, secureObjectId, userId, permissionId))
            throw new SecurityException("You need to grant permission!");
    }

    public async Task<SecureObject> BuildSystemEntity(int appId, Guid rootSecureObjectId)
    {
        var systemSecureObject = await _authRepo.FindRootSecureObject(appId);

        if (systemSecureObject == null)
        {
            await CreateSystemSecureObject(appId, rootSecureObjectId);
        }
        else
        {
            // Validate root secure object
            if (systemSecureObject.SecureObjectExternalId != rootSecureObjectId)
                throw new InvalidOperationException("Wrong RootSecureObjectId.");
        }

        var secureObject = await _authRepo.GetSecureObjectByExternalId(rootSecureObjectId);
        var secureObjectResult = secureObject.ToDto();
        return secureObjectResult;
    }

    private async Task<SecureObjectModel> GetRootSecureObject(int appId)
    {
        var rootSecureObject = await _authRepo.FindRootSecureObject(appId);
        return rootSecureObject ?? throw new Exception("Can not find the root secure object.");
    }

    private async Task CreateSystemSecureObject(int appId, Guid rootSecureObjectId)
    {
        var secureObjectType = new SecureObjectTypeModel
        {
            AppId = appId,
            SecureObjectTypeExternalId = Guid.NewGuid(),
            SecureObjectTypeName = "System"
        };
        await _authRepo.AddEntity(secureObjectType);
        await _authRepo.SaveChangesAsync();

        var secureObject = new SecureObjectModel
        {
            AppId = appId,
            SecureObjectExternalId = rootSecureObjectId,
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
                SecureObjectTypeExternalId = obValue.SecureObjectTypeId,
                SecureObjectTypeName = obValue.SecureObjectTypeName
            });

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.AppId == appId && x.SecureObjectTypeExternalId != c.SecureObjectTypeId)))
            _authRepo.RemoveEntity(dbValue);

        // update
        foreach (var dbValue in dbValues)
        {
            var obValue = obValues.SingleOrDefault(x => x.SecureObjectTypeId == dbValue.SecureObjectTypeExternalId);
            if (obValue == null) continue;
            dbValue.SecureObjectTypeName = obValue.SecureObjectTypeName;
        }
    }
}