using System.Security;
using Microsoft.Data.SqlClient;
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

    #region Permission

    public async Task<PermissionGroup[]> PermissionGroup_List(int appId)
    {
        return await _authDbContext.PermissionGroups
            .Where(x => x.AppId == appId)
            .Include(x => x.Permissions)
            .ToArrayAsync();
    }

    private async Task PermissionGroupPermission_Remove(int appId, Guid permissionGroupId)
    {
        // Get list PermissionGroupPermissions based on App and PermissionGroup
        var dbValues = await _authDbContext.PermissionGroupPermissions
                  .Where(x => x.AppId == appId && x.PermissionGroupId == permissionGroupId)
                  .ToListAsync();

        // Remove bulk
        _authDbContext.PermissionGroupPermissions.RemoveRange(dbValues);
    }

    private async Task Permission_UpdateBulk(int appId, PermissionDto[] obValues)
    {
        var dbValues = await _authDbContext.Permissions.Where(x => x.AppId == appId).ToListAsync();

        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => c.AppId == appId && x.PermissionCode != c.PermissionId)))
            await _authDbContext.Permissions.AddAsync(new Permission(appId, obValue.PermissionCode, obValue.PermissionName));

        // delete
        //foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.AppId == appId && x.PermissionId != c.PermissionCode)))
        //    _authDbContext.Permissions.Remove(dbValue);
        var deleteValues = dbValues.Where(x => obValues.All(c => x.AppId == appId && x.PermissionId != c.PermissionCode));
        _authDbContext.Permissions.RemoveRange(deleteValues);

        // update
        foreach (var dbValue in dbValues)
        {
            var obValue = obValues.SingleOrDefault(x => x.PermissionCode == dbValue.PermissionId);
            if (obValue == null) continue;
            dbValue.PermissionName = obValue.PermissionName;
        }
    }

    private void PermissionGroupPermission_UpdateBulk(ICollection<PermissionGroupPermission> dbValues, PermissionGroupPermission[] obValues)
    {
        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => x.PermissionId != c.PermissionId)))
            dbValues.Add(obValue);

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.PermissionId != c.PermissionId)))
            dbValues.Remove(dbValue);
    }

    private async Task PermissionGroup_UpdateBulk(int appId, PermissionGroupDto[] obValues, bool removeOthers)
    {
        try
        {
            var dbValues = await _authDbContext.PermissionGroups
                .Where(x => x.AppId == appId)
                .Include(x => x.PermissionGroupPermissions).ToListAsync();

            // add
            foreach (var obValue in obValues.Where(x => dbValues.All(c => x.PermissionGroupId != c.PermissionGroupId)))
            {
                var res = await _authDbContext.PermissionGroups.AddAsync(new PermissionGroup(appId, obValue.PermissionGroupId, obValue.PermissionGroupName));
                PermissionGroupPermission_UpdateBulk(res.Entity.PermissionGroupPermissions,
                    obValue.Permissions.Select(x => new PermissionGroupPermission { AppId = appId, PermissionGroupId = res.Entity.PermissionGroupId, PermissionId = x.PermissionCode }).ToArray());
            }

            // delete
            if (removeOthers)
                foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.PermissionGroupId != c.PermissionGroupId)))
                {
                    // Remove PermissionGroupPermission
                    await PermissionGroupPermission_Remove(appId, dbValue.PermissionGroupId);

                    // Remove PermissionGroup
                    _authDbContext.PermissionGroups.Remove(dbValue);
                }

            // update
            foreach (var dbValue in dbValues)
            {
                var obValue = obValues.SingleOrDefault(x => x.PermissionGroupId == dbValue.PermissionGroupId);
                if (obValue == null) continue;

                PermissionGroupPermission_UpdateBulk(dbValue.PermissionGroupPermissions,
                    obValue.Permissions.Select(x => new PermissionGroupPermission { AppId = appId, PermissionGroupId = dbValue.PermissionGroupId, PermissionId = x.PermissionCode }).ToArray());

                dbValue.PermissionGroupName = obValue.PermissionGroupName;
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    #endregion

    #region SecureObjectType

    public async Task<SecureObjectType[]> SecureObjectType_List(int appId)
    {
        return await _authDbContext.SecureObjectTypes
            .Where(x => x.AppId == appId)
            .ToArrayAsync();
    }

    #endregion

    #region App

    public async Task<AppDto> Init(int appId, SecureObjectTypeDto[] secureObjectTypes, PermissionDto[] permissions, PermissionGroupDto[] permissionGroups, bool removeOtherPermissionGroups = true)
    {
        // Validate SecureObjectTypes
        SecureObjectType_ValidateName(secureObjectTypes);

        // Prepare system secure object
        var secureObject = await SecureObject_CreateImp(appId, default, default, default, true);

        // Prepare SecureObjectTypes to add System to passed list
        secureObjectTypes = SecureObjectType_BuildSystemObject(appId, secureObjectTypes, secureObject.SecureObjectTypeId);

        // update types
        await SecureObjectType_UpdateBulk(appId, secureObjectTypes);
        await Permission_UpdateBulk(appId, permissions);
        await PermissionGroup_UpdateBulk(appId, permissionGroups, removeOtherPermissionGroups);

        // Table function
        await _authDbContext.Database.ExecuteSqlRawAsync(SecureObject_HierarchySql());
        await _authDbContext.SaveChangesAsync();

        var appData = new AppDto(secureObject.SecureObjectId);
        return appData;
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

    #endregion

    #region Role

    public async Task<Role[]> Role_List(int appId)
    {
        return await _authDbContext.Roles
            .Where(x => x.AppId == appId)
            .ToArrayAsync();
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
        await _authDbContext.SaveChangesAsync();    
        return role;
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

    public async Task<RoleUser[]> Role_Users(int appId, Guid roleId)
    {
        return await _authDbContext.RoleUsers
                .Where(x => x.AppId == appId && x.RoleId == roleId)
                .ToArrayAsync();
    }

    #endregion

    #region SecureObject

    public async Task<SecureObjectDto> SecureObject_Create(int appId, Guid secureObjectId, Guid secureObjectTypeId, Guid? parentSecureObjectId)
    {
        // Call worker
        var secureObject = await SecureObject_CreateImp(appId, secureObjectId, secureObjectTypeId, parentSecureObjectId, false);
        await _authDbContext.SaveChangesAsync();

        var result = new SecureObjectDto(secureObject.SecureObjectId, secureObject.SecureObjectTypeId, secureObject.ParentSecureObjectId);
        return result;
    }

    private async Task<SecureObjectDto> SecureObject_CreateImp(int appId, Guid secureObjectId, Guid secureObjectTypeId, Guid? parentSecureObjectId, bool initializeSystem)
    {
        if (initializeSystem == true)
        {
            var retSecureObject = await SecureObject_BuildSystemEntity(appId);
            SecureObjectDto secureObjectDto = new SecureObjectDto(retSecureObject.SecureObjectId, retSecureObject.SecureObjectTypeId, retSecureObject.ParentSecureObjectId);

            return secureObjectDto;
        }

        // Try to get parentSecureObjectId
        if (parentSecureObjectId == null)
        {
            var systemSecureObject = await SecureObject_DefaultParent(appId);
            if (systemSecureObject == null)
                throw new Exception("SystemSecureObject does not have valid value");

            // Set parentSecureObjectId
            parentSecureObjectId = systemSecureObject.SecureObjectId;
        }

        // Prepare SecureObject
        var secureObject = new SecureObject
        {
            AppId = appId,
            SecureObjectId = secureObjectId,
            SecureObjectTypeId = secureObjectTypeId,
            ParentSecureObjectId = parentSecureObjectId
        };
        await _authDbContext.SecureObjects.AddAsync(secureObject);

        var result = new SecureObjectDto(secureObject.SecureObjectId, secureObject.SecureObjectTypeId, secureObject.ParentSecureObjectId);
        return result;
    }

    public async Task<SecureObject[]> SecureObject_List(int appId)
    {
        return await _authDbContext.SecureObjects
           .Where(x => x.AppId == appId)
           .ToArrayAsync();
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

    private async Task<SecureObject> SecureObject_BuildSystemEntity(int appId)
    {
        SecureObject secureObject = new SecureObject();
        var systemSecureObject = await SecureObject_DefaultParent(appId);
        if (systemSecureObject == null)
        {
            var secureObjectTypeDto = new SecureObjectTypeDto(Guid.NewGuid(), "System");

            secureObject = new SecureObject
            {
                AppId = appId,
                SecureObjectId = Guid.NewGuid(),
                SecureObjectTypeId = secureObjectTypeDto.SecureObjectTypeId,
                ParentSecureObjectId = null
            };
            systemSecureObject = (await _authDbContext.SecureObjects.AddAsync(secureObject)).Entity;
        }
        else
        {
            secureObject = systemSecureObject;
        }
        return secureObject;
    }

    private async Task<SecureObject?> SecureObject_DefaultParent(int appId)
    {
        var systemSecureObject = await _authDbContext.SecureObjects.SingleOrDefaultAsync(x => x.AppId == appId && x.ParentSecureObjectId == null);
        return systemSecureObject;
    }

    private async Task SecureObjectType_UpdateBulk(int appId, SecureObjectTypeDto[] obValues)
    {
        try
        {
            // Get SecureObjectTypes from db
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
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void SecureObjectType_ValidateName(SecureObjectTypeDto[] secureObjectTypes)
    {
        // Validate for System value in list
        var result = secureObjectTypes.FirstOrDefault(x => x.SecureObjectTypeName == "System");
        if (result != null)
            throw new Exception("The SecureObjectTypeName could not allow System as an input parameter");
    }

    private SecureObjectTypeDto[] SecureObjectType_BuildSystemObject(int appId, SecureObjectTypeDto[] secureObjectTypes, Guid systemSecureObjectTypeId)
    {
        // System object must exists
        SecureObjectTypeDto secureObjectTypeDto;
        secureObjectTypeDto = new SecureObjectTypeDto(systemSecureObjectTypeId, "System");
        secureObjectTypes = secureObjectTypes.Concat(new[] { secureObjectTypeDto }).ToArray();
        return secureObjectTypes;
    }

    #endregion
}