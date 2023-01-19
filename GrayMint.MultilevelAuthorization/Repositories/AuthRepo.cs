using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Persistence;
using MultiLevelAuthorization.Services.Views;

namespace MultiLevelAuthorization.Repositories;

public class AuthRepo
{
    private readonly AuthDbContext _authDbContext;

    public AuthRepo(AuthDbContext authDbContext)
    {
        _authDbContext = authDbContext;
    }

    public async Task SaveChangesAsync()
    {
        await _authDbContext.SaveChangesAsync();
    }

    public async Task AddEntity<TEntity>(TEntity entity) where TEntity : class
    {
        await _authDbContext.Set<TEntity>().AddAsync(entity);
    }

    public void RemoveEntities<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
    {
        _authDbContext.Set<TEntity>().RemoveRange(entities);
    }

    public void RemoveEntity<TEntity>(TEntity entity) where TEntity : class
    {
        _authDbContext.Set<TEntity>().Remove(entity);
    }

    public async Task<AppModel> GetApp(int appId)
    {
        var appModel = await _authDbContext.Apps
            .SingleAsync(x => x.AppId == appId);
        return appModel;
    }

    public async Task<int> GetPermissionGroupIdByExternalId(int appId, Guid permissionGroupId)
    {
        var permissionGroupModel = await _authDbContext.PermissionGroups
            .SingleAsync(x => x.AppId == appId && x.PermissionGroupExternalId == permissionGroupId);

        return permissionGroupModel.PermissionGroupId;
    }

    public async Task<PermissionGroupModel[]> GetPermissionGroups(int appId)
    {
        return await _authDbContext.PermissionGroups
            .Where(x => x.AppId == appId)
            .Include(x => x.Permissions)
            .Include(x => x.PermissionGroupPermissions)
            .ToArrayAsync();
    }

    public async Task<PermissionModel[]> GetPermissions(int appId)
    {
        return await _authDbContext.Permissions
            .Where(x => x.AppId == appId)
            .ToArrayAsync();
    }

    public async Task<PermissionGroupPermissionModel[]> GetPermissionGroupPermissionsByPermissionGroupId(int appId, int permissionGroupId)
    {
        // Get list PermissionGroupPermissions based on App and PermissionGroup
        var permissionGroupPermissions = await _authDbContext.PermissionGroupPermissions
            .Where(x => x.AppId == appId && x.PermissionGroupId == permissionGroupId)
            .ToArrayAsync();

        return permissionGroupPermissions;
    }

    public async Task<SecureObjectModel> GetSecureObjectByExternalId(int appId, Guid secureObjectId)
    {
        var secureObject = await _authDbContext.SecureObjects
            .SingleAsync(x => x.AppId == appId && x.SecureObjectExternalId == secureObjectId);

        return secureObject;
    }

    public async Task<SecureObjectModel?> FindRootSecureObject(int appId)
    {
        var secureObject = await _authDbContext.SecureObjects
            .SingleOrDefaultAsync(x => x.AppId == appId && x.ParentSecureObjectId == null);

        return secureObject;
    }

    public async Task<PermissionModel[]> GetUserPermissions(int appId, int secureObjectId, Guid userId)
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

        // Select from db
        var dbQuery = query1
            .Union(query2)
            .Distinct();

        var ret = await dbQuery.ToArrayAsync();

        //  Change output to dtos
        var permissionModels = ret.Select(x => new PermissionModel
        {
            PermissionId = x.PermissionId,
            PermissionName = x.PermissionName
        }).ToArray();
        return permissionModels;
    }

    public async Task<SecureObjectTypeModel[]> GetSecureObjectTypes(int appId)
    {
        return await _authDbContext.SecureObjectTypes
            .Where(x => x.AppId == appId)
            .ToArrayAsync();
    }

    public async Task<SecureObjectView> GetSecureObjectByExternalId(Guid secureObjectId)
    {
        return await _authDbContext.SecureObjects
            .Where(x => x.SecureObjectExternalId == secureObjectId)
            .Include(x => x.ParentSecureObject)
            .Include(x => x.SecureObjectType)
            .Select(x => new SecureObjectView
            {
                SecureObjectExternalId = x.SecureObjectExternalId,
                ParentSecureObjectExternalId = x.ParentSecureObject!.SecureObjectExternalId,
                SecureObjectTypeId = x.SecureObjectType!.SecureObjectTypeExternalId

            })
            .SingleAsync();
    }

    public async Task<SecureObjectView[]> GetSecureObjects(int appId)
    {
        var query = (from secureObjects in _authDbContext.SecureObjects
                     join parentSecureObjects in _authDbContext.SecureObjects
                         on secureObjects.ParentSecureObjectId equals parentSecureObjects.SecureObjectId
                         into grouping1
                     from pso in grouping1.DefaultIfEmpty()
                     join secureObjectTypes in _authDbContext.SecureObjectTypes
                         on secureObjects.SecureObjectTypeId equals secureObjectTypes.SecureObjectTypeId
                         into grouping2
                     from sot in grouping2.DefaultIfEmpty()
                     where secureObjects.AppId == appId
                     select new SecureObjectView
                     {
                         SecureObjectExternalId = secureObjects.SecureObjectExternalId,
                         SecureObjectTypeId = sot.SecureObjectTypeExternalId,
                         ParentSecureObjectExternalId = pso.SecureObjectExternalId
                     });

        var result = await query.ToArrayAsync();
        return result;
    }

    public async Task<int> GetSecureObjectTypeIdByExternalId(int appId, Guid secureObjectTypeId)
    {
        var secureObjectType = await _authDbContext.SecureObjectTypes
            .SingleAsync(x => x.AppId == appId && x.SecureObjectTypeExternalId == secureObjectTypeId);

        return secureObjectType.SecureObjectTypeId;
    }
    public async Task<RoleModel[]> GetRoles(int appId)
    {
        return await _authDbContext.Roles
            .Where(x => x.AppId == appId)
            .ToArrayAsync();
    }
    public async Task<RoleUserModel[]> GetRoleUsers(int appId, Guid roleId)
    {
        var roleUserModels = await _authDbContext.RoleUsers
            .Where(x => x.AppId == appId && x.RoleId == roleId)
            .ToArrayAsync();

        return roleUserModels;
    }

    public async Task<RoleView[]> GetUserRoles(int appId, Guid userId)
    {
        var query = from roleUser in _authDbContext.RoleUsers
                    join roles in _authDbContext.Roles
                        on roleUser.RoleId equals roles.RoleId
                    where roleUser.AppId == appId && roleUser.UserId == userId
                    select new RoleView
                    {
                        RoleId = roles.RoleId,
                        RoleName = roles.RoleName,
                        ModifiedByUserId = roleUser.ModifiedByUserId,
                        OwnerId = roles.OwnerSecureObjectId
                    };

        var roleViews = await query.ToArrayAsync();
        return roleViews;
    }

    public async Task<int> GetNewAuthorizationCode()
    {
        var result = await _authDbContext.Apps
            .MaxAsync(x => (int?)x.AuthorizationCode);
        var maxAuthCode = result ?? 0;
        maxAuthCode++;
        return maxAuthCode;
    }

    public async Task Init()
    {
        await _authDbContext.Database.ExecuteSqlRawAsync(SecureObject_HierarchySql());
    }

    // SqlInjection safe by just id parameter as Guid
    private static string SecureObject_HierarchySql()
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
}