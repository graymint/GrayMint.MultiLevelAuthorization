using System.Security;
using GrayMint.MultiLevelAuthorization.Dtos;
using GrayMint.MultiLevelAuthorization.Models;
using GrayMint.MultiLevelAuthorization.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GrayMint.MultiLevelAuthorization.Repositories;

public class AuthRepo2
{
    private readonly AuthDbContext _authDbContext;
    public AuthRepo2(AuthDbContext dbContext)
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

    public async Task<PermissionGroupModel[]> PermissionGroup_List(string appId)
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

    private async Task Permission_UpdateBulk(int appId, Permission[] obValues)
    {
        var dbValues = await _authDbContext.Permissions.Where(x => x.AppId == appId).ToListAsync();

        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => c.AppId == appId && x.PermissionId != c.PermissionId)))
            await _authDbContext.Permissions.AddAsync(new PermissionModel(appId, obValue.PermissionId, obValue.PermissionName));

        // delete
        var deleteValues = dbValues.Where(x => obValues.All(c => x.AppId == appId && x.PermissionId != c.PermissionId));
        _authDbContext.Permissions.RemoveRange(deleteValues);

        // update
        foreach (var dbValue in dbValues)
        {
            var obValue = obValues.SingleOrDefault(x => x.PermissionId == dbValue.PermissionId);
            if (obValue == null) continue;
            dbValue.PermissionName = obValue.PermissionName;
        }
    }

    private void PermissionGroupPermission_UpdateBulk(ICollection<PermissionGroupPermissionModel> dbValues, PermissionGroupPermissionModel[] obValues)
    {
        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => x.PermissionId != c.PermissionId)))
            dbValues.Add(obValue);

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.PermissionId != c.PermissionId)))
            dbValues.Remove(dbValue);
    }

    private async Task PermissionGroup_UpdateBulk(int appId, PermissionGroup[] obValues, bool removeOthers)
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
                    new PermissionGroupModel(appId, obValue.PermissionGroupId, obValue.PermissionGroupName));
                PermissionGroupPermission_UpdateBulk(res.Entity.PermissionGroupPermissions,
                    obValue.Permissions.Select(x => new PermissionGroupPermissionModel { AppId = appId, PermissionGroupId = res.Entity.PermissionGroupId, PermissionId = x.PermissionId }).ToArray());
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
                    obValue.Permissions.Select(x => new PermissionGroupPermissionModel { AppId = appId, PermissionGroupId = dbValue.PermissionGroupId, PermissionId = x.PermissionId }).ToArray());

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

    private async Task<AppModel> App_Get(int appId)
    {
        var result = await _authDbContext.Apps
            .SingleAsync(x => x.AppId == appId);
        return result;
    }

    public async Task<App> App_Init(string appId, Guid rootSecureObjectId, SecureObjectType[] secureObjectTypes, Permission[] permissions, PermissionGroup[] permissionGroups, bool removeOtherPermissionGroups = true)
    {
        try
        {
            if (rootSecureObjectId == Guid.Empty)
                throw new InvalidOperationException("Can not set default guid for rootSecureObjectId");

            var dbAppId = await App_GetIdByName(appId);

            var appInfo = await App_Get(dbAppId);

            // Validate SecureObjectTypes for System value in list
            var result = secureObjectTypes.FirstOrDefault(x => x.SecureObjectTypeName == "System");
            if (result != null)
                throw new Exception("The SecureObjectTypeName could not allow System as an input parameter");

            // Prepare system secure object
            var secureObjectDto = await SecureObject_BuildSystemEntity(dbAppId, rootSecureObjectId);
            //SecureObjectDto secureObjectDto = new SecureObjectDto(secureObject.SecureObjectId, secureObject.SecureObjectTypeId, secureObject.ParentSecureObjectId);

            // Prepare SecureObjectTypes to add System to passed list
            SecureObjectType secureObjectType = new SecureObjectType(secureObjectDto.SecureObjectTypeId, "System");
            secureObjectTypes = secureObjectTypes.Concat(new[] { secureObjectType }).ToArray();

            // update types
            await SecureObjectType_UpdateBulk(dbAppId, secureObjectTypes);
            await Permission_UpdateBulk(dbAppId, permissions);
            await PermissionGroup_UpdateBulk(dbAppId, permissionGroups, removeOtherPermissionGroups);

            // Table function
            await _authDbContext.Database.ExecuteSqlRawAsync(SecureObject_HierarchySql());
            await _authDbContext.SaveChangesAsync();

            var appData = new App(appInfo.AppName, appInfo.AppDescription, secureObjectDto.SecureObjectId);
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
        var appEntity = (await _authDbContext.Apps.AddAsync(new AppModel()
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

    public async Task<RoleModel[]> Role_List(string appId)
    {
        var dbAppId = await App_GetIdByName(appId);

        return await _authDbContext.Roles
            .Where(x => x.AppId == dbAppId)
            .ToArrayAsync();
    }

    public async Task<Role> Role_Create(string appId, string roleName, Guid ownerId, Guid modifiedByUserId)
    {
        var dbAppId = await App_GetIdByName(appId);

        var role = new RoleModel
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
        Role role = new Role(role.RoleId, role.RoleName);
        return role;
    }

    public async Task Role_AddUser(string appId, Guid roleId, Guid userId, Guid modifiedByUserId)
    {
        var dbAppId = await App_GetIdByName(appId);

        //todo check permission on appId

        var roleUser = new RoleUserModel
        {
            AppId = dbAppId,
            RoleId = roleId,
            UserId = userId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId
        };
        await _authDbContext.RoleUsers.AddAsync(roleUser);
        await _authDbContext.SaveChangesAsync();
    }

    public async Task<List<User>> Role_Users(string appId, Guid roleId)
    {
        var dbAppId = await App_GetIdByName(appId);
        var result = await _authDbContext.RoleUsers
                .Where(x => x.AppId == dbAppId && x.RoleId == roleId)
                .ToListAsync();

        List<User> userDtos = new List<User>();
        foreach (var item in result)
        {
            userDtos.Add(new User(item.UserId));
        }

        return userDtos;
    }

    public async Task<List<Role>> User_Roles(string appId, Guid userId)
    {
        var dbAppId = await App_GetIdByName(appId);
        var query = from roleUser in _authDbContext.RoleUsers
                    join roles in _authDbContext.Roles
                    on roleUser.RoleId equals roles.RoleId
                    where roleUser.AppId == dbAppId && roleUser.UserId == userId
                    select new
                    {
                        roles.RoleId,
                        roles.RoleName
                    };

        var result = await query.ToListAsync();

        List<Role> roleDto = new List<Role>();
        foreach (var item in result)
        {
            roleDto.Add(new Role(item.RoleId, item.RoleName));
        }

        return roleDto;
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

    public async Task<SecureObject> SecureObject_Create(string appId, Guid secureObjectId, Guid secureObjectTypeId, Guid? parentSecureObjectId)
    {
        var dbAppId = await App_GetIdByName(appId);
        var dbSecureObjectTypeId = await SecureObjectType_GetIdByExternalId(dbAppId, secureObjectTypeId);

        // Call worker
        var secureObject = await SecureObject_CreateImp(dbAppId, secureObjectId, dbSecureObjectTypeId, parentSecureObjectId);
        await _authDbContext.SaveChangesAsync();

        var result = new SecureObject(secureObject.SecureObjectId, secureObject.SecureObjectTypeId, secureObject.ParentSecureObjectId);
        return result;
    }

    private async Task<SecureObject> SecureObject_CreateImp(int appId, Guid secureObjectId, int secureObjectTypeId, Guid? parentSecureObjectId)
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
        var secureObject = new SecureObjectModel
        {
            AppId = appId,
            SecureObjectExternalId = secureObjectId,
            SecureObjectTypeId = secureObjectTypeId,
            ParentSecureObjectId = dbParentSecureObjectId
        };
        await _authDbContext.SecureObjects.AddAsync(secureObject);

        var result = new SecureObject(secureObject.SecureObjectExternalId, secureObjectId, parentSecureObjectId);
        return result;
    }

    public async Task<List<SecureObject>> SecureObject_List(string appId)
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

        List<SecureObject> secureObjectDtos = new List<SecureObject>();
        foreach (var item in result)
        {
            SecureObject secureObject = new SecureObject(item.SecureObjectId, item.SecureObjectTypeId, item.ParentSecureObjectId);
            secureObjectDtos.Add(secureObject);
        }

        return secureObjectDtos;
    }

    public async Task<SecureObjectRolePermissionModel> SecureObject_AddRolePermission(string appId, Guid secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
    {
        var dbAppId = await App_GetIdByName(appId);
        var dbPermissionGroupId = await PermissionGroup_GetIdByExternalId(dbAppId, permissionGroupId);
        var dbSecureObjectId = await SecureObject_GetIdByExternalId(dbAppId, secureObjectId);

        //todo check permission on appId

        var secureObjectRolePermission = new SecureObjectRolePermissionModel
        {
            AppId = dbAppId,
            SecureObjectId = dbSecureObjectId,
            RoleId = roleId,
            PermissionGroupId = dbPermissionGroupId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
        };
        var ret = await _authDbContext.SecureObjectRolePermissions.AddAsync(secureObjectRolePermission);
        await _authDbContext.SaveChangesAsync();
        return ret.Entity;
    }

        public async Task<SecureObjectUserPermissionModel> SecureObject_AddUserPermission(string appId, Guid secureObjectId, Guid userId, Guid permissionGroupId,
            Guid modifiedByUserId)
    {
        var dbAppId = await App_GetIdByName(appId);
        var dbPermissionGroupId = await PermissionGroup_GetIdByExternalId(dbAppId, permissionGroupId);
        var dbSecureObjectId = await SecureObject_GetIdByExternalId(dbAppId, secureObjectId);

        //todo check permission on appId

        var secureObjectUserPermission = new SecureObjectUserPermissionModel
        {
            AppId = dbAppId,
            SecureObjectId = dbSecureObjectId,
            UserId = userId,
            PermissionGroupId = dbPermissionGroupId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
        };
        var ret = await _authDbContext.SecureObjectUserPermissions.AddAsync(secureObjectUserPermission);
        await _authDbContext.SaveChangesAsync();
        return ret.Entity;
    }

    // SqlInjection safe by just id parameter as Guid
    private static string SecureObject_HierarchySql()
    {
        var secureObjects = $"{AuthDbContext.Schema}.{nameof(AuthDbContext.SecureObjects)}";
        var secureObjectId = $"{nameof(SecureObjectModel.SecureObjectId)}";
        var parentSecureObjectId = $"{nameof(SecureObjectModel.ParentSecureObjectId)}";

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

    public async Task<List<Permission>> SecureObject_GetUserPermissions(string appId, Guid secureObjectId, Guid userId)
    {
        var dbAppId = await App_GetIdByName(appId);
        var dbSecureObjectId = await SecureObject_GetIdByExternalId(dbAppId,secureObjectId);
        var query1 =
            from secureObject in _authDbContext.SecureObjectHierarchy(dbSecureObjectId)
            join rolePermission in _authDbContext.SecureObjectRolePermissions on secureObject.SecureObjectId equals rolePermission.SecureObjectId
            join permissionGroupPermission in _authDbContext.PermissionGroupPermissions on rolePermission.PermissionGroupId equals permissionGroupPermission.PermissionGroupId
            join role in _authDbContext.Roles on rolePermission.RoleId equals role.RoleId
            join roleUser in _authDbContext.RoleUsers on role.RoleId equals roleUser.RoleId
            where roleUser.UserId == userId && role.AppId == dbAppId
            select permissionGroupPermission.Permission;

        var query2 =
            from secureObject in _authDbContext.SecureObjectHierarchy(dbSecureObjectId)
            join secureObjectType in _authDbContext.SecureObjectTypes on secureObject.SecureObjectTypeId equals secureObjectType.SecureObjectTypeId
            join userPermission in _authDbContext.SecureObjectUserPermissions on secureObject.SecureObjectId equals userPermission.SecureObjectId
            join permissionGroupPermission in _authDbContext.PermissionGroupPermissions on userPermission.PermissionGroupId equals permissionGroupPermission.PermissionGroupId
            where userPermission.UserId == userId && secureObjectType.AppId == dbAppId
            select permissionGroupPermission.Permission;

        // Select from db
        var ret = await query1
            .Union(query2)
            .Distinct()
            .ToArrayAsync();

        //  Change output to dtos
        List<Permission> permissionDtos = new List<Permission>();
        foreach (var item in ret)
        {
            Permission permission = new Permission(item.PermissionId, item.PermissionName);
            permissionDtos.Add(permission);
        }
        return permissionDtos;
    }

    public async Task<bool> SecureObject_HasUserPermission(string appId, Guid secureObjectId, Guid userId, int permissionId)
    {
        var permissions = await SecureObject_GetUserPermissions(appId, secureObjectId, userId);
        return permissions.Any(x => x.PermissionId == permissionId);
    }

    public async Task SecureObject_VerifyUserPermission(string appId, Guid secureObjectId, Guid userId, PermissionModel permissionModel)
    {
        if (secureObjectId == Guid.Empty)
            throw new SecurityException($"{nameof(secureObjectId)} can not be empty!");

        if (!await SecureObject_HasUserPermission(appId, secureObjectId, userId, permissionModel.PermissionId))
            throw new SecurityException($"You need to grant {permissionModel.PermissionName} permission!");
    }

    private async Task<SecureObject> SecureObject_BuildSystemEntity(int appId, Guid rootSecureObjectId)
    {
        var systemSecureObject = await _authDbContext.SecureObjects.
            SingleOrDefaultAsync(x => x.AppId == appId && x.ParentSecureObjectId == null);
        Guid rootSecureObjectTypeId = Guid.NewGuid();

        if (systemSecureObject == null)
        {
            var secureObjectType = new SecureObjectTypeModel(appId, rootSecureObjectTypeId, "System");
            await _authDbContext.SecureObjectTypes.AddAsync(secureObjectType);
            await _authDbContext.SaveChangesAsync();
            var secureObjectTypeId = secureObjectType.SecureObjectTypeId;

            var secureObject = new SecureObjectModel
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
            // Retrieve secure object external id based on pk id
            var secureObjectType = await _authDbContext.SecureObjectTypes
                .SingleAsync(x => x.SecureObjectTypeId == systemSecureObject.SecureObjectTypeId);

            // Vaidate root secure object
            if ( systemSecureObject.SecureObjectExternalId != rootSecureObjectId)
                throw new InvalidOperationException("In this app, RootSecureObjectId is incompatible with saved data.");

            // Set rootSecureObjectTypeId
            rootSecureObjectTypeId = secureObjectType.SecureObjectTypeExternalId;
        }

        SecureObject secureObject = new SecureObject(
            rootSecureObjectId, rootSecureObjectTypeId, null);
        return secureObject;
    }

    private async Task SecureObjectType_UpdateBulk(int appId, SecureObjectType[] obValues)
    {
        // Get SecureObjectTypes from db
        var dbValues = await _authDbContext.SecureObjectTypes.Where(x => x.AppId == appId).ToListAsync();

        // add
        foreach (var obValue in obValues.Where(x =>
                     dbValues.All(c => c.AppId == appId && x.SecureObjectTypeId != c.SecureObjectTypeExternalId)))
            await _authDbContext.SecureObjectTypes.AddAsync(new SecureObjectTypeModel(appId, obValue.SecureObjectTypeId, obValue.SecureObjectTypeName));

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

    public async Task<SecureObjectTypeModel[]> SecureObjectType_List(string appId)
    {
        var dbAppId = await App_GetIdByName(appId);

        return await _authDbContext.SecureObjectTypes
            .Where(x => x.AppId == dbAppId)
            .ToArrayAsync();
    }

    #endregion
}