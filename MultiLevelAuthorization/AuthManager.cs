using System.Security;
using Microsoft.EntityFrameworkCore;
using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization;

public class AuthManager
{
    private readonly AuthDbContext _dbContext;
    private readonly short _appId;

    public AuthManager(AuthDbContext dbContext, short appId)
    {
        _dbContext = dbContext;
        _appId = appId;
    }

    public async Task<AppDto> Init(SecureObjectTypeDto[] secureObjectTypes, PermissionDto[] permissions, PermissionGroupDto[] permissionGroups, bool removeOtherPermissionGroups = true)
    {
        // System object must exists
        var systemSecureObject = await _dbContext.SecureObjects.SingleOrDefaultAsync(x => x.SecureObjectType!.AppId == _appId && x.ParentSecureObjectId == null);
        if (systemSecureObject == null)
        {
            var secureObjectType = (await _dbContext.SecureObjectTypes.AddAsync(new SecureObjectType(_appId, Guid.NewGuid(), "System"))).Entity;
            systemSecureObject = (await _dbContext.SecureObjects.AddAsync(new SecureObject
            {
                AppId = _appId,
                SecureObjectId = Guid.NewGuid(),
                SecureObjectTypeId = secureObjectType.SecureObjectTypeId,
                ParentSecureObjectId = null
            })).Entity;
        }

        // update types
        await UpdateSecureObjectTypes(secureObjectTypes);
        await UpdatePermissions(permissions);
        await UpdatePermissionGroups(permissionGroups, removeOtherPermissionGroups);

        // Table function
        await _dbContext.Database.ExecuteSqlRawAsync(SecureObject_HierarchySql());
        await _dbContext.SaveChangesAsync();

        var appData = new AppDto(systemSecureObject.SecureObjectId);
        return appData;
    }

    private async Task UpdateSecureObjectTypes(SecureObjectTypeDto[] obValues)
    {
        var dbValues = await _dbContext.SecureObjectTypes.Where(x => x.AppId == _appId).ToListAsync();

        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => c.AppId == _appId && x.SecureObjectTypeId != c.SecureObjectTypeId)))
            await _dbContext.SecureObjectTypes.AddAsync(new SecureObjectType(_appId, obValue.SecureObjectTypeId, obValue.SecureObjectTypeName));

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.AppId == _appId && x.SecureObjectTypeId != c.SecureObjectTypeId)))
            _dbContext.SecureObjectTypes.Remove(dbValue);

        // update
        foreach (var dbValue in dbValues)
        {
            var obValue = obValues.SingleOrDefault(x => x.SecureObjectTypeId == dbValue.SecureObjectTypeId);
            if (obValue == null) continue;
            dbValue.SecureObjectTypeName = obValue.SecureObjectTypeName;
        }
    }

    private async Task UpdatePermissions(PermissionDto[] obValues)
    {
        var dbValues = await _dbContext.Permissions.Where(x => x.AppId == _appId).ToListAsync();

        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => c.AppId == _appId && x.PermissionCode != c.PermissionId)))
            await _dbContext.Permissions.AddAsync(new Permission(_appId, obValue.PermissionCode, obValue.PermissionName));

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.AppId == _appId && x.PermissionId != c.PermissionCode)))
            _dbContext.Permissions.Remove(dbValue);

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

    private async Task UpdatePermissionGroups(PermissionGroupDto[] obValues, bool removeOthers)
    {
        var dbValues = await _dbContext.PermissionGroups
            .Where(x => x.AppId == _appId)
            .Include(x => x.PermissionGroupPermissions).ToListAsync();

        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => x.PermissionGroupId != c.PermissionGroupId)))
        {
            var res = await _dbContext.PermissionGroups.AddAsync(new PermissionGroup(_appId, obValue.PermissionGroupId, obValue.PermissionGroupName));
            UpdatePermissionGroupPermissions(res.Entity.PermissionGroupPermissions,
                obValue.Permissions.Select(x => new PermissionGroupPermission { AppId = _appId, PermissionGroupId = res.Entity.PermissionGroupId, PermissionId = x.PermissionCode }).ToArray());
        }

        // delete
        if (removeOthers)
            foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.PermissionGroupId != c.PermissionGroupId)))
                _dbContext.PermissionGroups.Remove(dbValue);

        // update
        foreach (var dbValue in dbValues)
        {
            var obValue = obValues.SingleOrDefault(x => x.PermissionGroupId == dbValue.PermissionGroupId);
            if (obValue == null) continue;

            UpdatePermissionGroupPermissions(dbValue.PermissionGroupPermissions,
                obValue.Permissions.Select(x => new PermissionGroupPermission { AppId = _appId, PermissionGroupId = dbValue.PermissionGroupId, PermissionId = x.PermissionCode }).ToArray());

            dbValue.PermissionGroupName = obValue.PermissionGroupName;
        }
    }

    public async Task<PermissionGroup[]> GetPermissionGroups()
    {
        return await _dbContext.PermissionGroups
            .Where(x => x.AppId == _appId)
            .Include(x => x.Permissions)
            .ToArrayAsync();
    }

    public async Task<SecureObject> CreateSecureObject(Guid secureObjectId, Guid secureObjectTypeId)
    {
        var systemSecureObject = await _dbContext.SecureObjects.SingleAsync(x => x.SecureObjectType!.AppId == _appId && x.ParentSecureObjectId == null);
        return await CreateSecureObject(secureObjectId, secureObjectTypeId, systemSecureObject.SecureObjectId);
    }

    public async Task<SecureObject> CreateSecureObject(Guid secureObjectId, Guid secureObjectTypeId, Guid parentSecureObjectId)
    {
        //todo check permission on appId

        var secureObject = new SecureObject
        {
            AppId = _appId,
            SecureObjectId = secureObjectId,
            SecureObjectTypeId = secureObjectTypeId,
            ParentSecureObjectId = parentSecureObjectId
        };
        await _dbContext.SecureObjects.AddAsync(secureObject);
        return secureObject;
    }

    public async Task<Role> Role_Create(string roleName, Guid ownerId, Guid modifiedByUserId)
    {
        var role = new Role
        {
            AppId = _appId,
            OwnerId = ownerId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
            RoleId = Guid.NewGuid(),
            RoleName = roleName
        };
        await _dbContext.Roles.AddAsync(role);
        return role;
    }

    public Task Role_AddUser(Role role, Guid userId, Guid modifiedByUserId)
    {
        return Role_AddUser(role.RoleId, userId, modifiedByUserId);
    }

    public async Task Role_AddUser(Guid roleId, Guid userId, Guid modifiedByUserId)
    {
        //todo check permission on appId

        var roleUser = new RoleUser
        {
            AppId = _appId,
            RoleId = roleId,
            UserId = userId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId
        };
        await _dbContext.RoleUsers.AddAsync(roleUser);
    }

    public async Task<SecureObjectRolePermission> SecureObject_AddRolePermission(Guid secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        //todo check permission on appId

        var secureObjectRolePermission = new SecureObjectRolePermission
        {
            AppId = _appId,
            SecureObjectId = secureObjectId,
            RoleId = roleId,
            PermissionGroupId = permissionGroupId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
        };
        var ret = await _dbContext.SecureObjectRolePermissions.AddAsync(secureObjectRolePermission);
        return ret.Entity;
    }

    public async Task<SecureObjectUserPermission> SecureObject_AddUserPermission(Guid secureObjectId, Guid userId, Guid permissionGroupId,
        Guid modifiedByUserId)
    {
        //todo check permission on appId

        var secureObjectUserPermission = new SecureObjectUserPermission
        {
            AppId = _appId,
            SecureObjectId = secureObjectId,
            UserId = userId,
            PermissionGroupId = permissionGroupId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
        };
        var ret = await _dbContext.SecureObjectUserPermissions.AddAsync(secureObjectUserPermission);
        return ret.Entity;
    }

    // SqlInjection safe by just id parameter as Guid
    public static string SecureObject_HierarchySql()
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

    public async Task<SecureObjectRolePermission[]> SecureObject_GetRolePermissionGroups(Guid secureObjectId)
    {
        var ret = await _dbContext.SecureObjectRolePermissions
            .Include(x => x.Role)
            .Where(x => x.SecureObjectId == secureObjectId && x.SecureObject!.SecureObjectType!.AppId == _appId)
            .ToArrayAsync();
        return ret;
    }

    public async Task<SecureObjectUserPermission[]> SecureObject_GetUserPermissionGroups(Guid secureObjectId)
    {
        var ret = await _dbContext.SecureObjectUserPermissions
            .Where(x => x.SecureObjectId == secureObjectId && x.SecureObject!.SecureObjectType!.AppId == _appId)
            .ToArrayAsync();
        return ret;
    }

    public async Task<Permission[]> SecureObject_GetUserPermissions(Guid secureObjectId, Guid userId)
    {
        var query1 =
            from secureObject in _dbContext.SecureObjectHierarchy(secureObjectId)
            join rolePermission in _dbContext.SecureObjectRolePermissions on secureObject.SecureObjectId equals rolePermission.SecureObjectId
            join permissionGroupPermission in _dbContext.PermissionGroupPermissions on rolePermission.PermissionGroupId equals permissionGroupPermission.PermissionGroupId
            join role in _dbContext.Roles on rolePermission.RoleId equals role.RoleId
            join roleUser in _dbContext.RoleUsers on role.RoleId equals roleUser.RoleId
            where roleUser.UserId == userId && role.AppId == _appId
            select permissionGroupPermission.Permission;

        var query2 =
            from secureObject in _dbContext.SecureObjectHierarchy(secureObjectId)
            join secureObjectType in _dbContext.SecureObjectTypes on secureObject.SecureObjectTypeId equals secureObjectType.SecureObjectTypeId
            join userPermission in _dbContext.SecureObjectUserPermissions on secureObject.SecureObjectId equals userPermission.SecureObjectId
            join permissionGroupPermission in _dbContext.PermissionGroupPermissions on userPermission.PermissionGroupId equals permissionGroupPermission.PermissionGroupId
            where userPermission.UserId == userId && secureObjectType.AppId == _appId
            select permissionGroupPermission.Permission;


        var ret = await query1
            .Union(query2)
            .Distinct()
            .ToArrayAsync();

        return ret;
    }

    public Task<bool> SecureObject_HasUserPermission(Guid secureObjectId, Guid userId, Permission permission)
    {
        return SecureObject_HasUserPermission(secureObjectId, userId, permission.PermissionId);
    }

    public async Task<bool> SecureObject_HasUserPermission(Guid secureObjectId, Guid userId, int permissionId)
    {
        var permissions = await SecureObject_GetUserPermissions(secureObjectId, userId);
        return permissions.Any(x => x.PermissionId == permissionId);
    }

    public async Task SecureObject_VerifyUserPermission(Guid secureObjectId, Guid userId, Permission permission)
    {
        if (secureObjectId == Guid.Empty)
            throw new SecurityException($"{nameof(secureObjectId)} can not be empty!");

        if (!await SecureObject_HasUserPermission(secureObjectId, userId, permission.PermissionId))
            throw new SecurityException($"You need to grant {permission.PermissionName} permission!");
    }
}