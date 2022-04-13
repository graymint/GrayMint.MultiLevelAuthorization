﻿using System.Security;
using Microsoft.EntityFrameworkCore;
using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Persistence;

namespace MultiLevelAuthorization.Repositories;

public class AuthManager
{
    private readonly AuthDbContext _authDbContext;
    public AuthManager(AuthDbContext dbContext)
    {
        _authDbContext = dbContext;
    }

    #region Permission
    private async Task<int> PermissionGroup_GetIdByExternalId(int appId, Guid permissionGroupId)
    {
        var result = await _authDbContext.PermissionGroups
            .SingleAsync(x => x.AppId == appId && x.PermissionGroupExternalId == permissionGroupId);

        return result.PermissionGroupId;
    }

    public async Task<PermissionGroup[]> PermissionGroup_List(string appId)
    {
        var dbAppId = await App_GetIdByName(appId);

        return await _authDbContext.PermissionGroups
            .Where(x => x.AppId == dbAppId)
            .Include(x => x.Permissions)
            .ToArrayAsync();
    }

    private async Task PermissionGroupPermission_Remove(int appId, int permissionGroupId)
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
            foreach (var obValue in obValues.Where(x => dbValues.All(c => x.PermissionGroupId != c.PermissionGroupExternalId)))
            {
                var res = await _authDbContext.PermissionGroups.AddAsync(
                    new PermissionGroup(appId, obValue.PermissionGroupId, obValue.PermissionGroupName));
                PermissionGroupPermission_UpdateBulk(res.Entity.PermissionGroupPermissions,
                    obValue.Permissions.Select(x => new PermissionGroupPermission { AppId = appId, PermissionGroupId = res.Entity.PermissionGroupId, PermissionId = x.PermissionCode }).ToArray());
            }

            // delete
            if (removeOthers)
                foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.PermissionGroupExternalId != c.PermissionGroupId)))
                {
                    // Remove PermissionGroupPermission
                    await PermissionGroupPermission_Remove(appId, dbValue.PermissionGroupId);

                    // Remove PermissionGroup
                    _authDbContext.PermissionGroups.Remove(dbValue);
                }

            // update
            foreach (var dbValue in dbValues)
            {
                var obValue = obValues.SingleOrDefault(x => x.PermissionGroupId == dbValue.PermissionGroupExternalId);
                if (obValue == null) continue;

                PermissionGroupPermission_UpdateBulk(dbValue.PermissionGroupPermissions,
                    obValue.Permissions.Select(x => new PermissionGroupPermission { AppId = appId, PermissionGroupId = dbValue.PermissionGroupId, PermissionId = x.PermissionCode }).ToArray());

                dbValue.PermissionGroupName = obValue.PermissionGroupName;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    #endregion

    #region App


    private async Task<App> App_Get(int appId)
    {
        var result = await _authDbContext.Apps
            .SingleAsync(x => x.AppId == appId);
        return result;
    }

    public async Task<AppDto> App_Init(string appId, Guid rootSecureObjectId, Guid rootSecureObjectTypeId, SecureObjectTypeDto[] secureObjectTypes, PermissionDto[] permissions, PermissionGroupDto[] permissionGroups, bool removeOtherPermissionGroups = true)
    {
        try
        {
            if (rootSecureObjectId == Guid.Empty || rootSecureObjectTypeId == Guid.Empty)
                throw new InvalidOperationException("Coud not set defaut huid for rootSecureObjectId and rootSecureObjectTypeId");

            var dbAppId = await App_GetIdByName(appId);

            var appInfo = await App_Get(dbAppId);

            // Validate SecureObjectTypes for System value in list
            var result = secureObjectTypes.FirstOrDefault(x => x.SecureObjectTypeName == "System");
            if (result != null)
                throw new Exception("The SecureObjectTypeName could not allow System as an input parameter");

            // Prepare system secure object
            var secureObjectDto = await SecureObject_BuildSystemEntity(dbAppId, rootSecureObjectId, rootSecureObjectTypeId);
            //SecureObjectDto secureObjectDto = new SecureObjectDto(secureObject.SecureObjectId, secureObject.SecureObjectTypeId, secureObject.ParentSecureObjectId);

            // Prepare SecureObjectTypes to add System to passed list
            SecureObjectTypeDto secureObjectTypeDto = new SecureObjectTypeDto(secureObjectDto.SecureObjectTypeId, "System");
            secureObjectTypes = secureObjectTypes.Concat(new[] { secureObjectTypeDto }).ToArray();

            // update types
            await SecureObjectType_UpdateBulk(dbAppId, secureObjectTypes);
            await Permission_UpdateBulk(dbAppId, permissions);
            await PermissionGroup_UpdateBulk(dbAppId, permissionGroups, removeOtherPermissionGroups);

            // Table function
            await _authDbContext.Database.ExecuteSqlRawAsync(SecureObject_HierarchySql());
            await _authDbContext.SaveChangesAsync();

            var appData = new AppDto(appInfo.AppName, appInfo.AppDescription, secureObjectDto.SecureObjectId);
            return appData;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<string> App_Create(AppCreateRequestHandler request)
    {
        // Build new app id
        var newAppId = await App_NewId();

        // Create auth.App
        var appEntity = (await _authDbContext.Apps.AddAsync(new App()
        {
            AppId = newAppId,
            AppName = request.AppName,
            AppDescription = request.AppDescription
        })).Entity;
        await _authDbContext.SaveChangesAsync();

        return appEntity.AppName;
    }
    private async Task<int> App_GetIdByName(string appId)
    {
        var result = await _authDbContext.Apps
            .SingleAsync(x => x.AppName == appId);

        return result.AppId;
    }

    private async Task<int> App_NewId()
    {
        var result = await _authDbContext.Apps
            .OrderByDescending(x => x.AppId)
            .FirstOrDefaultAsync();
        var newAppId = (result == null) ? 1 : result.AppId + 1;
        return newAppId;
    }

    #endregion

    #region Role

    public async Task<Role[]> Role_List(string appId)
    {
        var dbAppId = await App_GetIdByName(appId);

        return await _authDbContext.Roles
            .Where(x => x.AppId == dbAppId)
            .ToArrayAsync();
    }

    public async Task<Role> Role_Create(string appId, string roleName, Guid ownerId, Guid modifiedByUserId)
    {
        var dbAppId = await App_GetIdByName(appId);

        var role = new Role
        {
            AppId = dbAppId,
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

    public async Task Role_AddUser(string appId, Guid roleId, Guid userId, Guid modifiedByUserId)
    {
        var dbAppId = await App_GetIdByName(appId);

        //todo check permission on appId

        var roleUser = new RoleUser
        {
            AppId = dbAppId,
            RoleId = roleId,
            UserId = userId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId
        };
        await _authDbContext.RoleUsers.AddAsync(roleUser);
    }

    public async Task<RoleUser[]> Role_Users(string appId, Guid roleId)
    {
        var dbAppId = await App_GetIdByName(appId);

        return await _authDbContext.RoleUsers
                .Where(x => x.AppId == dbAppId && x.RoleId == roleId)
                .ToArrayAsync();
    }

    #endregion

    #region SecureObject

    private async Task<int> SecureObject_GetIdByExternalId(int appId, Guid secureObjectId)
    {
        var result = await _authDbContext.SecureObjects
            .SingleAsync(x => x.AppId == appId && x.SecureObjectExternalId == secureObjectId);

        return result.SecureObjectId;
    }

    private async Task<int> SecureObjectType_GetIdByExternalId(int appId, Guid secureObjectTypeId)
    {
        var result = await _authDbContext.SecureObjectTypes
            .SingleAsync(x => x.AppId == appId && x.SecureObjectTypeExternalId == secureObjectTypeId);

        return result.SecureObjectTypeId;
    }

    public async Task<SecureObjectDto> SecureObject_Create(string appId, Guid secureObjectId, Guid secureObjectTypeId, Guid? parentSecureObjectId)
    {
        var dbAppId = await App_GetIdByName(appId);
        var dbSecureObjectTypeId = await SecureObjectType_GetIdByExternalId(dbAppId, secureObjectTypeId);

        // Call worker
        var secureObject = await SecureObject_CreateImp(dbAppId, secureObjectId, dbSecureObjectTypeId, parentSecureObjectId);
        await _authDbContext.SaveChangesAsync();

        var result = new SecureObjectDto(secureObject.SecureObjectId, secureObject.SecureObjectTypeId, secureObject.ParentSecureObjectId);
        return result;
    }

    private async Task<SecureObjectDto> SecureObject_CreateImp(int appId, Guid secureObjectId, int secureObjectTypeId, Guid? parentSecureObjectId)
    {
        int dbParentSecureObjectId;
        // Try to get parentSecureObjectId
        if (parentSecureObjectId == null)
        {
            // Make sure system secure object has been created
            var systemSecureObject = await _authDbContext.SecureObjects.SingleOrDefaultAsync(x => x.AppId == appId && x.ParentSecureObjectId == null);

            if (systemSecureObject == null)
                throw new Exception("SystemSecureObject does not have valid value");

            // Set parentSecureObjectId
            dbParentSecureObjectId = systemSecureObject.SecureObjectId;
            parentSecureObjectId = systemSecureObject.SecureObjectExternalId;
        }
        else
        {
            var retSecureObject = await _authDbContext.SecureObjects.SingleAsync(
                x => x.SecureObjectExternalId == parentSecureObjectId);
            dbParentSecureObjectId = retSecureObject.SecureObjectId;
        }

        // Prepare SecureObject
        var secureObject = new SecureObject
        {
            AppId = appId,
            SecureObjectExternalId = secureObjectId,
            SecureObjectTypeId = secureObjectTypeId,
            ParentSecureObjectId = dbParentSecureObjectId
        };
        await _authDbContext.SecureObjects.AddAsync(secureObject);

        var result = new SecureObjectDto(secureObject.SecureObjectExternalId, secureObjectId, parentSecureObjectId);
        return result;
    }

    public async Task<List<SecureObjectDto>> SecureObject_List(string appId)
    {
        var dbAppId = await App_GetIdByName(appId);

        var query = (from secureObjects in _authDbContext.SecureObjects
                     join parentSecureObjects in _authDbContext.SecureObjects
                         on secureObjects.ParentSecureObjectId equals parentSecureObjects.SecureObjectId
                         into grouping1
                     from pso in grouping1.DefaultIfEmpty()
                     join secureObjectTypes in _authDbContext.SecureObjectTypes
                         on secureObjects.SecureObjectTypeId equals secureObjectTypes.SecureObjectTypeId
                         into grouping2
                     from sot in grouping2.DefaultIfEmpty()
                     where secureObjects.AppId == dbAppId
                     select new
                     {
                         SecureObjectId = secureObjects.SecureObjectExternalId,
                         SecureObjectTypeId = sot.SecureObjectTypeExternalId,
                         ParentSecureObjectId = (Guid?)pso.SecureObjectExternalId
                     });


        var result = await query.ToListAsync();

        List<SecureObjectDto> secureObjectDtos = new List<SecureObjectDto>();
        foreach (var item in result)
        {
            SecureObjectDto secureObjectDto = new SecureObjectDto(item.SecureObjectId, item.SecureObjectTypeId, item.ParentSecureObjectId);
            secureObjectDtos.Add(secureObjectDto);
        }

        return secureObjectDtos;
    }

    public async Task<SecureObjectRolePermission> SecureObject_AddRolePermission(string appId, Guid secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        var dbAppId = await App_GetIdByName(appId);
        var dbPermissionGroupId = await PermissionGroup_GetIdByExternalId(dbAppId, permissionGroupId);
        var dbSecureObjectId = await SecureObject_GetIdByExternalId(dbAppId, secureObjectId);

        //todo check permission on appId

        var secureObjectRolePermission = new SecureObjectRolePermission
        {
            AppId = dbAppId,
            SecureObjectId = dbSecureObjectId,
            RoleId = roleId,
            PermissionGroupId = dbPermissionGroupId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
        };
        var ret = await _authDbContext.SecureObjectRolePermissions.AddAsync(secureObjectRolePermission);
        return ret.Entity;
    }

    public async Task<SecureObjectUserPermission> SecureObject_AddUserPermission(string appId, Guid secureObjectId, Guid userId, Guid permissionGroupId,
        Guid modifiedByUserId)
    {
        var dbAppId = await App_GetIdByName(appId);
        var dbPermissionGroupId = await PermissionGroup_GetIdByExternalId(dbAppId, permissionGroupId);
        var dbSecureObjectId = await SecureObject_GetIdByExternalId(dbAppId, secureObjectId);

        //todo check permission on appId

        var secureObjectUserPermission = new SecureObjectUserPermission
        {
            AppId = dbAppId,
            SecureObjectId = dbSecureObjectId,
            UserId = userId,
            PermissionGroupId = dbPermissionGroupId,
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

    public async Task<SecureObjectRolePermission[]> SecureObject_GetRolePermissionGroups(string appId, Guid secureObjectId)
    {
        var dbAppId = await App_GetIdByName(appId);
        var dbSecureObjectId = await SecureObject_GetIdByExternalId(dbAppId, secureObjectId);

        var ret = await _authDbContext.SecureObjectRolePermissions
            .Include(x => x.Role)
            .Where(x => x.SecureObjectId == dbSecureObjectId && x.SecureObject!.SecureObjectType!.AppId == dbAppId)
            .ToArrayAsync();
        return ret;
    }

    public async Task<SecureObjectUserPermission[]> SecureObject_GetUserPermissionGroups(string appId, Guid secureObjectId)
    {
        var dbAppId = await App_GetIdByName(appId);
        var dbSecureObjectId = await SecureObject_GetIdByExternalId(dbAppId, secureObjectId);

        var ret = await _authDbContext.SecureObjectUserPermissions
            .Where(x => x.SecureObjectId == dbSecureObjectId && x.SecureObject!.SecureObjectType!.AppId == dbAppId)
            .ToArrayAsync();
        return ret;
    }

    public async Task<Permission[]> SecureObject_GetUserPermissions(string appId, Guid secureObjectId, Guid userId)
    {
        var dbAppId = await App_GetIdByName(appId);

        var query1 =
            from secureObject in _authDbContext.SecureObjectHierarchy(secureObjectId)
            join rolePermission in _authDbContext.SecureObjectRolePermissions on secureObject.SecureObjectId equals rolePermission.SecureObjectId
            join permissionGroupPermission in _authDbContext.PermissionGroupPermissions on rolePermission.PermissionGroupId equals permissionGroupPermission.PermissionGroupId
            join role in _authDbContext.Roles on rolePermission.RoleId equals role.RoleId
            join roleUser in _authDbContext.RoleUsers on role.RoleId equals roleUser.RoleId
            where roleUser.UserId == userId && role.AppId == dbAppId
            select permissionGroupPermission.Permission;

        var query2 =
            from secureObject in _authDbContext.SecureObjectHierarchy(secureObjectId)
            join secureObjectType in _authDbContext.SecureObjectTypes on secureObject.SecureObjectTypeId equals secureObjectType.SecureObjectTypeId
            join userPermission in _authDbContext.SecureObjectUserPermissions on secureObject.SecureObjectId equals userPermission.SecureObjectId
            join permissionGroupPermission in _authDbContext.PermissionGroupPermissions on userPermission.PermissionGroupId equals permissionGroupPermission.PermissionGroupId
            where userPermission.UserId == userId && secureObjectType.AppId == dbAppId
            select permissionGroupPermission.Permission;


        var ret = await query1
            .Union(query2)
            .Distinct()
            .ToArrayAsync();

        return ret;
    }

    public async Task<bool> SecureObject_HasUserPermission(string appId, Guid secureObjectId, Guid userId, int permissionId)
    {
        var permissions = await SecureObject_GetUserPermissions(appId, secureObjectId, userId);
        return permissions.Any(x => x.PermissionId == permissionId);
    }

    public async Task SecureObject_VerifyUserPermission(string appId, Guid secureObjectId, Guid userId, Permission permission)
    {
        if (secureObjectId == Guid.Empty)
            throw new SecurityException($"{nameof(secureObjectId)} can not be empty!");

        if (!await SecureObject_HasUserPermission(appId, secureObjectId, userId, permission.PermissionId))
            throw new SecurityException($"You need to grant {permission.PermissionName} permission!");
    }

    private async Task<SecureObjectDto> SecureObject_BuildSystemEntity(int appId, Guid rootSecureObjectId, Guid rootSecureObjectTypeId)
    {
        var systemSecureObject = await _authDbContext.SecureObjects.
            SingleOrDefaultAsync(x => x.AppId == appId && x.ParentSecureObjectId == null);

        if (systemSecureObject == null)
        {
            var secureObjectType = new SecureObjectType(appId, rootSecureObjectTypeId, "System");
            await _authDbContext.SecureObjectTypes.AddAsync(secureObjectType);
            await _authDbContext.SaveChangesAsync();
            var secureObjectTypeId = secureObjectType.SecureObjectTypeId;

            var secureObject = new SecureObject
            {
                AppId = appId,
                SecureObjectExternalId = rootSecureObjectId,
                SecureObjectTypeId = secureObjectTypeId,
                ParentSecureObjectId = null
            };
            await _authDbContext.SecureObjects.AddAsync(secureObject);
        }
        else
        {
            var dbRootSecureObjectTypeId = await SecureObjectType_GetIdByExternalId(appId, rootSecureObjectTypeId);

            // Vaidate root secure object
            if (systemSecureObject.SecureObjectTypeId != dbRootSecureObjectTypeId || systemSecureObject.SecureObjectExternalId != rootSecureObjectId)
                throw new InvalidOperationException("In this app, RootSecureObjectId is incompatible with saved data.");
        }

        SecureObjectDto secureObjectDto = new SecureObjectDto(
            rootSecureObjectId, rootSecureObjectTypeId, null);
        return secureObjectDto;
    }

    private async Task SecureObjectType_UpdateBulk(int appId, SecureObjectTypeDto[] obValues)
    {
        // Get SecureObjectTypes from db
        var dbValues = await _authDbContext.SecureObjectTypes.Where(x => x.AppId == appId).ToListAsync();

        // add
        foreach (var obValue in obValues.Where(x =>
                     dbValues.All(c => c.AppId == appId && x.SecureObjectTypeId != c.SecureObjectTypeExternalId)))
            await _authDbContext.SecureObjectTypes.AddAsync(new SecureObjectType(appId, obValue.SecureObjectTypeId, obValue.SecureObjectTypeName));

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.AppId == appId && x.SecureObjectTypeExternalId != c.SecureObjectTypeId)))
            _authDbContext.SecureObjectTypes.Remove(dbValue);

        // update
        foreach (var dbValue in dbValues)
        {
            var obValue = obValues.SingleOrDefault(x => x.SecureObjectTypeId == dbValue.SecureObjectTypeExternalId);
            if (obValue == null) continue;
            dbValue.SecureObjectTypeName = obValue.SecureObjectTypeName;
        }
    }

    public async Task<SecureObjectType[]> SecureObjectType_List(string appId)
    {
        var dbAppId = await App_GetIdByName(appId);

        return await _authDbContext.SecureObjectTypes
            .Where(x => x.AppId == dbAppId)
            .ToArrayAsync();
    }

    #endregion
}