using System.Security;
using MultiLevelAuthorization.DtoConverters;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Persistence;
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

        // Try to get parentSecureObjectId
        int dbParentSecureObjectId;
        if (parentSecureObjectId == null)
        {
            // Make sure system secure object has been created
            var systemSecureObject = await _authRepo.FindRootSecureObject(appId);
            ArgumentNullException.ThrowIfNull(systemSecureObject);

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
        var secureObjectModel = new SecureObjectModel
        {
            AppId = appId,
            SecureObjectExternalId = secureObjectId,
            SecureObjectTypeId = dbSecureObjectTypeId,
            ParentSecureObjectId = dbParentSecureObjectId
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

    public async Task<SecureObject[]> GetSecureObjects(int appId)
    {
        var secureObjects = await _authRepo.GetSecureObjects(appId);
        return secureObjects.Select(x => x.ToDto()).ToArray();
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

    public async Task SecureObject_VerifyUserPermission(int appId, Guid secureObjectId, Guid userId, PermissionModel permissionModel)
    {
        if (!await HasUserPermission(appId, secureObjectId, userId, permissionModel.PermissionId))
            throw new SecurityException($"You need to grant {permissionModel.PermissionName} permission!");
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

    private async Task CreateSystemSecureObject(int appId, Guid rootSecureObjectId)
    {
        var secureObjectType = new SecureObjectTypeModel
        {
            AppId = appId,
            SecureObjectTypeExternalId = new Guid(),
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

    public async Task<SecureObjectType[]> GetSecureObjectTypes(int appId)
    {
        var secureObjectTypeModels = await _authRepo.GetSecureObjectTypes(appId);
        return secureObjectTypeModels.Select(x => x.ToDto()).ToArray();
    }
}