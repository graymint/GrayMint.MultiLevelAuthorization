using System.Security;
using MultiLevelAuthorization.DtoConverters;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Persistence;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Services;

public class SecureObjectService
{
    private readonly AuthRepo3 _authRepo;
    private readonly PermissionService _permissionService;

    public SecureObjectService(
        AuthRepo3 authRepo,
        PermissionService permissionService
        )
    {
        _authRepo = authRepo;
        _permissionService = permissionService;
    }

    public async Task<int> GetIdByExternalId(int appId, Guid secureObjectId)
    {
        var secureObjectIdResult = await _authRepo.GetSecureObjectByExternalId(appId, secureObjectId);
        return secureObjectIdResult.SecureObjectId;
    }

    public async Task<SecureObject> Create(int appId, Guid secureObjectId, Guid secureObjectTypeId, Guid? parentSecureObjectId)
    {
        var dbSecureObjectTypeId = await GetSecureObjectTypeIdByExternalId(appId, secureObjectTypeId);

        // Call worker
        var secureObject = await CreateImp(appId, secureObjectId, dbSecureObjectTypeId, parentSecureObjectId);
        await _authRepo.SaveChangesAsync();

        var result = new SecureObject
        {
            SecureObjectId = secureObject.SecureObjectId,
            SecureObjectTypeId = secureObject.SecureObjectTypeId,
            ParentSecureObjectId = secureObject.ParentSecureObjectId
        };
        return result;
    }

    private async Task<SecureObject> CreateImp(int appId, Guid secureObjectId, int secureObjectTypeId, Guid? parentSecureObjectId)
    {
        int dbParentSecureObjectId;
        // Try to get parentSecureObjectId
        if (parentSecureObjectId == null)
        {
            // Make sure system secure object has been created
            var systemSecureObject = await _authRepo.GetRootSecureObject(appId);

            if (systemSecureObject == null)
                throw new Exception("SystemSecureObject does not have valid value");

            // Set parentSecureObjectId
            dbParentSecureObjectId = systemSecureObject.SecureObjectId;
            parentSecureObjectId = systemSecureObject.SecureObjectExternalId;
        }
        else
        {
            var retSecureObject = await _authRepo.GetSecureObjectByExternalId(appId, (Guid)parentSecureObjectId);
            dbParentSecureObjectId = retSecureObject.SecureObjectId;
        }

        // Prepare SecureObject
        var secureObject = new SecureObjectModel
        {
            AppId = appId,
            SecureObjectExternalId = secureObjectId,
            SecureObjectTypeId = secureObjectTypeId,
            ParentSecureObjectId = dbParentSecureObjectId
        };
        await _authRepo.AddEntity(secureObject);

        var result = new SecureObject
        {
            SecureObjectId = secureObject.SecureObjectExternalId,
            SecureObjectTypeId = secureObject.SecureObjectType.SecureObjectTypeExternalId,
            ParentSecureObjectId = parentSecureObjectId
        };
        return result;
    }

    public async Task<SecureObject[]> GetSecureObjects(int appId)
    {
        var secureObjects = await _authRepo.GetSecureObjects(appId);
        return secureObjects.Select(x => x.ToDto()).ToArray();
    }

    public async Task AddRolePermission(int appId, Guid secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        var dbPermissionGroupId = await _permissionService.GetPermissionGroupIdByExternalId(appId, permissionGroupId);
        var dbSecureObjectId = await GetSecureObjectTypeIdByExternalId(appId, secureObjectId);

        var secureObjectRolePermission = new SecureObjectRolePermissionModel
        {
            AppId = appId,
            SecureObjectId = dbSecureObjectId,
            RoleId = roleId,
            PermissionGroupId = dbPermissionGroupId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
        };

        await _authRepo.AddEntity(secureObjectRolePermission);
        await _authRepo.SaveChangesAsync();
    }

    public async Task AddUserPermission(int appId, Guid secureObjectId, Guid userId, Guid permissionGroupId,
        Guid modifiedByUserId)
    {
        var dbPermissionGroupId = await _permissionService.GetPermissionGroupIdByExternalId(appId, permissionGroupId);
        var dbSecureObjectId = await GetSecureObjectTypeIdByExternalId(appId, secureObjectId);

        var secureObjectUserPermission = new SecureObjectUserPermissionModel
        {
            AppId = appId,
            SecureObjectId = dbSecureObjectId,
            UserId = userId,
            PermissionGroupId = dbPermissionGroupId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
        };
        await _authRepo.AddEntity(secureObjectUserPermission);
        await _authRepo.SaveChangesAsync();
    }

    // SqlInjection safe by just id parameter as Guid
    public string SecureObject_HierarchySql()
    {
        const string secureObjects = $"{AuthDbContext.Schema}.{nameof(AuthDbContext.SecureObjects)}";
        const string secureObjectId = $"{nameof(SecureObjectModel.SecureObjectId)}";
        const string parentSecureObjectId = $"{nameof(SecureObjectModel.ParentSecureObjectId)}";

        var sql = @$"
					WITH SecureObjectParents
					AS (SELECT SO.*
						FROM {secureObjects} AS SO
						WHERE SO.{secureObjectId} = @id
						UNION ALL
						SELECT SO.*
						FROM {secureObjects} AS SO
							INNER JOIN SecureObjectParents AS PSO
								ON SO.{secureObjectId} = PSO.{parentSecureObjectId}
					   )
					SELECT SOP.*
					FROM SecureObjectParents AS SOP
					UNION
					SELECT * FROM {secureObjects} AS SO
					WHERE SO.{secureObjectId} = @id
					".Replace("                    ", "    ");

        var createSql =
            $"CREATE OR ALTER FUNCTION [{AuthDbContext.Schema}].[{nameof(AuthDbContext.SecureObjectHierarchy)}](@id int)\r\nRETURNS TABLE\r\nAS\r\nRETURN\r\n({sql})";

        return createSql;
    }

    public async Task<bool> HasUserPermission(int appId, Guid secureObjectId, Guid userId, int permissionId)
    {
        var permissions = await _authRepo.GetUserPermissions(appId, secureObjectId, userId);
        return permissions.Any(x => x.PermissionId == permissionId);
    }

    public async Task<Permission[]> GetUserPermissions(int appId, Guid secureObjectId, Guid userId)
    {
        var permissions = await _authRepo.GetUserPermissions(appId, secureObjectId, userId);
        return permissions.Select(x => x.ToDto()).ToArray();
    }

    public async Task SecureObject_VerifyUserPermission(int appId, Guid secureObjectId, Guid userId, PermissionModel permissionModel)
    {
        if (secureObjectId == Guid.Empty)
            throw new SecurityException($"{nameof(secureObjectId)} can not be empty!");

        if (!await HasUserPermission(appId, secureObjectId, userId, permissionModel.PermissionId))
            throw new SecurityException($"You need to grant {permissionModel.PermissionName} permission!");
    }

    public async Task<SecureObject> BuildSystemEntity(int appId, Guid rootSecureObjectId)
    {
        var systemSecureObject = await _authRepo.GetRootSecureObject(appId);
        var rootSecureObjectTypeId = Guid.NewGuid();

        if (systemSecureObject == null)
        {
            var secureObjectType = new SecureObjectTypeModel
            {
                AppId = appId,
                SecureObjectTypeExternalId = rootSecureObjectTypeId,
                SecureObjectTypeName = "System"
            };
            await _authRepo.AddEntity(secureObjectType);
            await _authRepo.SaveChangesAsync();
            var secureObjectTypeId = secureObjectType.SecureObjectTypeId;

            var secureObject = new SecureObjectModel
            {
                AppId = appId,
                SecureObjectExternalId = rootSecureObjectId,
                SecureObjectTypeId = secureObjectTypeId,
                ParentSecureObjectId = null
            };
            await _authRepo.AddEntity(secureObject);
        }
        else
        {
            // Retrieve secure object external id based on pk id
            var secureObjectType = await _authRepo.GetSecureObjectType(systemSecureObject.SecureObjectTypeId);

            // Validate root secure object
            if (systemSecureObject.SecureObjectExternalId != rootSecureObjectId)
                throw new InvalidOperationException("In this app, RootSecureObjectId is incompatible with saved data.");

            // Set rootSecureObjectTypeId
            rootSecureObjectTypeId = secureObjectType.SecureObjectTypeExternalId;
        }

        var secureObjectResult = new SecureObject
        {
            SecureObjectId = rootSecureObjectId,
            SecureObjectTypeId = rootSecureObjectTypeId,
            ParentSecureObjectId = null
        };
        return secureObjectResult;
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

    public async Task<SecureObjectType[]> GetSecureObjectTypes(int appId)
    {
        var secureObjectTypeModels = await _authRepo.GetSecureObjectTypes(appId);
        return secureObjectTypeModels.Select(x => x.ToDto()).ToArray();
    }

    public async Task<int> GetSecureObjectTypeIdByExternalId(int appId, Guid secureObjectTypeId)
    {
        var result = await _authRepo.GetSecureObjectTypeIdByExternalId(appId, secureObjectTypeId);
        return result;
    }
}