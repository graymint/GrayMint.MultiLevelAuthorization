using Microsoft.EntityFrameworkCore;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Persistence;

namespace MultiLevelAuthorization.Repositories;

public class AuthRepo
{
    private readonly AuthDbContext _authDbContext;

    public AuthRepo(AuthDbContext authDbContext)
    {
        _authDbContext = authDbContext;
    }

    public async Task BeginTransaction()
    {
        await _authDbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitTransaction()
    {
        await _authDbContext.Database.CommitTransactionAsync();
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

    public async Task<SecureObjectUserPermissionModel[]> GetSecureObjectUserPermissions(int appId, int secureObjectId, Guid userId)
    {
        var secureObjectUserPermission = await _authDbContext.SecureObjectUserPermissions
            .Include(x => x.PermissionGroup)
            .ThenInclude(x => x!.Permissions)
            .Where(x =>
                x.AppId == appId &&
                x.SecureObjectId == secureObjectId &&
                x.UserId == userId
            )
            .ToArrayAsync();

        return secureObjectUserPermission;
    }

    public async Task<SecureObjectRolePermissionModel[]> GetSecureObjectRolePermissions(int appId, int secureObjectId, Guid roleId)
    {
        var secureObjectRolePermission = await _authDbContext.SecureObjectRolePermissions
            .Include(x => x.PermissionGroup)
            .ThenInclude(x => x!.Permissions)
            .Where(x =>
                x.AppId == appId &&
                x.SecureObjectId == secureObjectId &&
                x.RoleId == roleId
            )
            .ToArrayAsync();

        return secureObjectRolePermission;
    }

    public async Task<SecureObjectModel> GetSecureObjectByExternalId(int appId, string secureObjectTypeId, string secureObjectId)
    {
        var secureObject = await _authDbContext.SecureObjects
            .Include(x => x.SecureObjectType)
            .SingleAsync(x =>
                x.AppId == appId &&
                x.SecureObjectExternalId == secureObjectId &&
                x.SecureObjectType!.SecureObjectTypeExternalId == secureObjectTypeId);

        return secureObject;
    }
    public async Task<SecureObjectModel?> FindSecureObjectByExternalId(int appId, string secureObjectTypeId, string secureObjectId)
    {
        var secureObject = await _authDbContext.SecureObjects
            .Include(x => x.SecureObjectType)
            .SingleOrDefaultAsync(x =>
                x.AppId == appId &&
                x.SecureObjectExternalId == secureObjectId &&
                x.SecureObjectType!.SecureObjectTypeExternalId == secureObjectTypeId);

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

    public async Task<int> GetSecureObjectTypeIdByExternalId(int appId, string secureObjectTypeId)
    {
        var secureObjectType = await _authDbContext.SecureObjectTypes
            .SingleAsync(x => x.AppId == appId && x.SecureObjectTypeExternalId == secureObjectTypeId);

        return secureObjectType.SecureObjectTypeId;
    }
    public async Task<RoleModel[]> GetRoles(int appId, int secureObjectId)
    {
        return await _authDbContext.Roles
            .Include(x => x.OwnerSecureObject)
            .ThenInclude(x => x!.SecureObjectType)
            .Where(x => x.AppId == appId && x.SecureObjectId == secureObjectId)
            .ToArrayAsync();
    }

    public async Task<RoleModel> GetRole(int appId, Guid roleId)
    {
        return await _authDbContext.Roles
            .Include(x=>x.RoleUsers)
            .Include(x => x.OwnerSecureObject)
            .ThenInclude(x => x!.SecureObjectType)
            .Where(x => x.AppId == appId && x.RoleId == roleId)
            .SingleAsync();
    }

    public async Task<RoleUserModel[]> GetUserRoles(int appId, Guid userId)
    {
        var userRoles = await _authDbContext.RoleUsers
            .Include(x => x.Role)
            .ThenInclude(x => x!.OwnerSecureObject)
            .ThenInclude(x => x!.SecureObjectType)
            .Where(x => x.AppId == appId && x.UserId == userId)
            .ToArrayAsync();

        return userRoles;
    }

    public async Task<int> GetNewAuthorizationCode()
    {
        var result = await _authDbContext.Apps
            .MaxAsync(x => (int?)x.AuthorizationCode);
        var maxAuthCode = result ?? 0;
        maxAuthCode++;
        return maxAuthCode;
    }

    public async Task ExecuteSqlRaw(string sqlCommand)
    {
        await _authDbContext.Database.ExecuteSqlRawAsync(sqlCommand);
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

    public string AppClearCommand(int appId)
    {
        // delete command
        var sql =
            $"DELETE {AuthDbContext.Schema}.{nameof(_authDbContext.RoleUsers)} WHERE {nameof(AppModel.AppId)} = {appId}" +
            $"DELETE {AuthDbContext.Schema}.{nameof(_authDbContext.SecureObjectRolePermissions)} WHERE {nameof(AppModel.AppId)} = {appId}" +
            $"DELETE {AuthDbContext.Schema}.{nameof(_authDbContext.Roles)} WHERE {nameof(AppModel.AppId)} = {appId}" +
            $"DELETE {AuthDbContext.Schema}.{nameof(_authDbContext.SecureObjectUserPermissions)} WHERE {nameof(AppModel.AppId)} = {appId}" +
            $"DELETE {AuthDbContext.Schema}.{nameof(_authDbContext.SecureObjects)} WHERE {nameof(AppModel.AppId)} = {appId}" +
            $"DELETE {AuthDbContext.Schema}.{nameof(_authDbContext.SecureObjectTypes)} WHERE {nameof(AppModel.AppId)} = {appId}" +
            $"DELETE {AuthDbContext.Schema}.{nameof(_authDbContext.Permissions)} WHERE {nameof(AppModel.AppId)} = {appId}" +
            $"DELETE {AuthDbContext.Schema}.{nameof(_authDbContext.PermissionGroups)} WHERE {nameof(AppModel.AppId)} = {appId}";

        return sql;
    }
}