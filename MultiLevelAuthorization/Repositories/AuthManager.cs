using System.Security;
using Microsoft.EntityFrameworkCore;
using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Persistence;

namespace MultiLevelAuthorization.Repositories;

public class AuthManager
{
    protected readonly AuthDbContext _authDbContext;
    public AuthManager(AuthDbContext dbContext)
    {
        _authDbContext = dbContext;
    }

    public async Task<AppDto> Init(int appId, SecureObjectTypeDto[] secureObjectTypes, PermissionDto[] permissions, PermissionGroupDto[] permissionGroups, bool removeOtherPermissionGroups = true)
    {
        // System object must exists
        var systemSecureObject = await _authDbContext.SecureObjects.SingleOrDefaultAsync(x => x.SecureObjectType!.AppId == appId && x.ParentSecureObjectId == null);
        if (systemSecureObject == null)
        {
            var secureObjectType = (await _authDbContext.SecureObjectTypes.AddAsync(new SecureObjectType(appId, Guid.NewGuid(), "System"))).Entity;
            systemSecureObject = (await _authDbContext.SecureObjects.AddAsync(new SecureObject
            {
                AppId = appId,
                SecureObjectId = Guid.NewGuid(),
                SecureObjectTypeId = secureObjectType.SecureObjectTypeId,
                ParentSecureObjectId = null
            })).Entity;
        }

        // update types
        await UpdateSecureObjectTypes(appId, secureObjectTypes);
        await UpdatePermissions(appId, permissions);
        await UpdatePermissionGroups(appId, permissionGroups, removeOtherPermissionGroups);

        // Table function
        await _authDbContext.Database.ExecuteSqlRawAsync(SecureObject_HierarchySql());
        await _authDbContext.SaveChangesAsync();

        var appData = new AppDto(systemSecureObject.SecureObjectId);
        return appData;
    }

    private async Task UpdateSecureObjectTypes(int appId, SecureObjectTypeDto[] obValues)
    {
        var dbValues = await _authDbContext.SecureObjectTypes.Where(x => x.AppId == appId).ToListAsync();

        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => c.AppId == appId && x.SecureObjectTypeId != c.SecureObjectTypeId)))
            await _authDbContext.SecureObjectTypes.AddAsync(new SecureObjectType(appId, obValue.SecureObjectTypeId, obValue.SecureObjectTypeName));

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.AppId == appId && x.SecureObjectTypeId != c.SecureObjectTypeId)))
            _authDbContext.SecureObjectTypes.Remove(dbValue);

        // update
        foreach (var dbValue in dbValues)
        {
            var obValue = obValues.SingleOrDefault(x => x.SecureObjectTypeId == dbValue.SecureObjectTypeId);
            if (obValue == null) continue;
            dbValue.SecureObjectTypeName = obValue.SecureObjectTypeName;
        }
    }

    private async Task UpdatePermissions(int appId, PermissionDto[] obValues)
    {
        var dbValues = await _authDbContext.Permissions.Where(x => x.AppId == appId).ToListAsync();

        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => c.AppId == appId && x.PermissionCode != c.PermissionId)))
            await _authDbContext.Permissions.AddAsync(new Permission(appId, obValue.PermissionCode, obValue.PermissionName));

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.AppId == appId && x.PermissionId != c.PermissionCode)))
            _authDbContext.Permissions.Remove(dbValue);

        // update
        foreach (var dbValue in dbValues)
        {
            var obValue = obValues.SingleOrDefault(x => x.PermissionCode == dbValue.PermissionId);
            if (obValue == null) continue;
            dbValue.PermissionName = obValue.PermissionName;
        }
    }

    private void UpdatePermissionGroupPermissions(ICollection<PermissionGroupPermission> dbValues, PermissionGroupPermission[] obValues)
    {
        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => x.PermissionId != c.PermissionId)))
            dbValues.Add(obValue);

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.PermissionId != c.PermissionId)))
            dbValues.Remove(dbValue);
    }

    private async Task UpdatePermissionGroups(int appId, PermissionGroupDto[] obValues, bool removeOthers)
    {
        var dbValues = await _authDbContext.PermissionGroups
            .Where(x => x.AppId == appId)
            .Include(x => x.PermissionGroupPermissions).ToListAsync();

        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => x.PermissionGroupId != c.PermissionGroupId)))
        {
            var res = await _authDbContext.PermissionGroups.AddAsync(new PermissionGroup(appId, obValue.PermissionGroupId, obValue.PermissionGroupName));
            UpdatePermissionGroupPermissions(res.Entity.PermissionGroupPermissions,
                obValue.Permissions.Select(x => new PermissionGroupPermission { AppId = appId, PermissionGroupId = res.Entity.PermissionGroupId, PermissionId = x.PermissionCode }).ToArray());
        }

        // delete
        if (removeOthers)
            foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.PermissionGroupId != c.PermissionGroupId)))
                _authDbContext.PermissionGroups.Remove(dbValue);

        // update
        foreach (var dbValue in dbValues)
        {
            var obValue = obValues.SingleOrDefault(x => x.PermissionGroupId == dbValue.PermissionGroupId);
            if (obValue == null) continue;

            UpdatePermissionGroupPermissions(dbValue.PermissionGroupPermissions,
                obValue.Permissions.Select(x => new PermissionGroupPermission { AppId = appId, PermissionGroupId = dbValue.PermissionGroupId, PermissionId = x.PermissionCode }).ToArray());

            dbValue.PermissionGroupName = obValue.PermissionGroupName;
        }
    }

    public async Task<SecureObjectType[]> GetSecureObjectTypes(int appId)
    {
        return await _authDbContext.SecureObjectTypes
            .Where(x => x.AppId == appId)
            .ToArrayAsync();
    }
    public async Task<PermissionGroup[]> GetPermissionGroups(int appId)
    {
        return await _authDbContext.PermissionGroups
            .Where(x => x.AppId == appId)
            .Include(x => x.Permissions)
            .ToArrayAsync();
    }

    public async Task<SecureObject> CreateSecureObject(int appId, Guid secureObjectId, Guid secureObjectTypeId)
    {
        var systemSecureObject = await _authDbContext.SecureObjects.SingleAsync(x => x.SecureObjectType!.AppId == appId && x.ParentSecureObjectId == null);
        return await CreateSecureObject(appId, secureObjectId, secureObjectTypeId, systemSecureObject.SecureObjectId);
    }

    public async Task<SecureObject> CreateSecureObject(int appId, Guid secureObjectId, Guid secureObjectTypeId, Guid parentSecureObjectId)
    {
        //todo check permission on appId

        var secureObject = new SecureObject
        {
            AppId = appId,
            SecureObjectId = secureObjectId,
            SecureObjectTypeId = secureObjectTypeId,
            ParentSecureObjectId = parentSecureObjectId
        };
        await _authDbContext.SecureObjects.AddAsync(secureObject);
        return secureObject;
    }

    public async Task<Role> Role_Create(int appId, string roleName, Guid ownerId, Guid modifiedByUserId)
    {
        var role = new Role
        {
            AppId = appId,
            OwnerId = ownerId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
            RoleId = Guid.NewGuid(),
            RoleName = roleName
        };
        await _authDbContext.Roles.AddAsync(role);
        return role;
    }

    public Task Role_AddUser(int appId, Role role, Guid userId, Guid modifiedByUserId)
    {
        return Role_AddUser(appId, role.RoleId, userId, modifiedByUserId);
    }

    public async Task Role_AddUser(int appId, Guid roleId, Guid userId, Guid modifiedByUserId)
    {
        //todo check permission on appId

        var roleUser = new RoleUser
        {
            AppId = appId,
            RoleId = roleId,
            UserId = userId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId
        };
        await _authDbContext.RoleUsers.AddAsync(roleUser);
    }

    public async Task<SecureObjectRolePermission> SecureObject_AddRolePermission(int appId, Guid secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        //todo check permission on appId

        var secureObjectRolePermission = new SecureObjectRolePermission
        {
            AppId = appId,
            SecureObjectId = secureObjectId,
            RoleId = roleId,
            PermissionGroupId = permissionGroupId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
        };
        var ret = await _authDbContext.SecureObjectRolePermissions.AddAsync(secureObjectRolePermission);
        return ret.Entity;
    }

    public async Task<SecureObjectUserPermission> SecureObject_AddUserPermission(int appId, Guid secureObjectId, Guid userId, Guid permissionGroupId,
        Guid modifiedByUserId)
    {
        //todo check permission on appId

        var secureObjectUserPermission = new SecureObjectUserPermission
        {
            AppId = appId,
            SecureObjectId = secureObjectId,
            UserId = userId,
            PermissionGroupId = permissionGroupId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
        };
        var ret = await _authDbContext.SecureObjectUserPermissions.AddAsync(secureObjectUserPermission);
        return ret.Entity;
    }

    // SqlInjection safe by just id parameter as Guid
    private static string SecureObject_HierarchySql()
    {
        var secureObjects = $"{AuthDbContext.Schema}.{nameof(AuthDbContext.SecureObjects)}";
        var secureObjectId = $"{nameof(SecureObject.SecureObjectId)}";
        var parentSecureObjectId = $"{nameof(SecureObject.ParentSecureObjectId)}";

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
            $"CREATE OR ALTER FUNCTION [{AuthDbContext.Schema}].[{nameof(AuthDbContext.SecureObjectHierarchy)}](@id varchar(40))\r\nRETURNS TABLE\r\nAS\r\nRETURN\r\n({sql})";

        return createSql;
    }

    public async Task<SecureObjectRolePermission[]> SecureObject_GetRolePermissionGroups(int appId, Guid secureObjectId)
    {
        var ret = await _authDbContext.SecureObjectRolePermissions
            .Include(x => x.Role)
            .Where(x => x.SecureObjectId == secureObjectId && x.SecureObject!.SecureObjectType!.AppId == appId)
            .ToArrayAsync();
        return ret;
    }

    public async Task<SecureObjectUserPermission[]> SecureObject_GetUserPermissionGroups(int appId, Guid secureObjectId)
    {
        var ret = await _authDbContext.SecureObjectUserPermissions
            .Where(x => x.SecureObjectId == secureObjectId && x.SecureObject!.SecureObjectType!.AppId == appId)
            .ToArrayAsync();
        return ret;
    }

    public async Task<Permission[]> SecureObject_GetUserPermissions(int appId, Guid secureObjectId, Guid userId)
    {
        var query1 =
            from secureObject in _authDbContext.SecureObjectHierarchy(secureObjectId)
            join rolePermission in _authDbContext.SecureObjectRolePermissions on secureObject.SecureObjectId equals rolePermission.SecureObjectId
            join permissionGroupPermission in _authDbContext.PermissionGroupPermissions on rolePermission.PermissionGroupId equals permissionGroupPermission.PermissionGroupId
            join role in _authDbContext.Roles on rolePermission.RoleId equals role.RoleId
            join roleUser in _authDbContext.RoleUsers on role.RoleId equals roleUser.RoleId
            where roleUser.UserId == userId && role.AppId == appId
            select permissionGroupPermission.Permission;

        var query2 =
            from secureObject in _authDbContext.SecureObjectHierarchy(secureObjectId)
            join secureObjectType in _authDbContext.SecureObjectTypes on secureObject.SecureObjectTypeId equals secureObjectType.SecureObjectTypeId
            join userPermission in _authDbContext.SecureObjectUserPermissions on secureObject.SecureObjectId equals userPermission.SecureObjectId
            join permissionGroupPermission in _authDbContext.PermissionGroupPermissions on userPermission.PermissionGroupId equals permissionGroupPermission.PermissionGroupId
            where userPermission.UserId == userId && secureObjectType.AppId == appId
            select permissionGroupPermission.Permission;


        var ret = await query1
            .Union(query2)
            .Distinct()
            .ToArrayAsync();

        return ret;
    }

    public Task<bool> SecureObject_HasUserPermission(int appId, Guid secureObjectId, Guid userId, Permission permission)
    {
        return SecureObject_HasUserPermission(appId, secureObjectId, userId, permission.PermissionId);
    }

    public async Task<bool> SecureObject_HasUserPermission(int appId, Guid secureObjectId, Guid userId, int permissionId)
    {
        var permissions = await SecureObject_GetUserPermissions(appId, secureObjectId, userId);
        return permissions.Any(x => x.PermissionId == permissionId);
    }

    public async Task SecureObject_VerifyUserPermission(int appId, Guid secureObjectId, Guid userId, Permission permission)
    {
        if (secureObjectId == Guid.Empty)
            throw new SecurityException($"{nameof(secureObjectId)} can not be empty!");

        if (!await SecureObject_HasUserPermission(appId, secureObjectId, userId, permission.PermissionId))
            throw new SecurityException($"You need to grant {permission.PermissionName} permission!");
    }

    public async Task<string> App_Create(AppCreateRequestHandler request)
    {
        var maxApp = await GetMaxApp();
        // Create auth.App
        var appEntity = (await _authDbContext.Apps.AddAsync(new App()
        {
            AppId = (maxApp == null) ? 1 : maxApp.AppId + 1,
            AppName = request.AppName,
            AppDescription = request.AppDescription
        })).Entity;
        await _authDbContext.SaveChangesAsync();

        return appEntity.AppName;
    }
    public async Task<App> App_PropsByName(string appId)
    {
        var result = await _authDbContext.Apps
            .SingleAsync(x => x.AppName == appId);

        return result;
    }

    private async Task<App> GetMaxApp()
    {
        var result = await _authDbContext.Apps
            .OrderByDescending(x => x.AppId)
            .FirstOrDefaultAsync();

        return result;
    }
}