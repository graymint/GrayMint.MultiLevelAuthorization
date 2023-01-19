using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Services;

public class PermissionService
{
    private readonly AuthRepo _authRepo;

    public PermissionService(AuthRepo authRepo)
    {
        _authRepo = authRepo;
    }
    public async Task<int> GetPermissionGroupIdByExternalId(int appId, Guid permissionGroupId)
    {
        var permissionGroupIdResult = await _authRepo.GetPermissionGroupIdByExternalId(appId, permissionGroupId);
        return permissionGroupIdResult;
    }

    private async Task RemovePermissionGroupPermission(int appId, int permissionGroupId)
    {
        // Get list PermissionGroupPermissions
        var permissionGroupPermissions = await _authRepo.GetPermissionGroupPermissionsByPermissionGroup(appId, permissionGroupId);

        // Remove
        _authRepo.RemoveEntities(permissionGroupPermissions);
    }

    public async Task Update(int appId, Permission[] obValues)
    {
        var dbValues = await _authRepo.GetPermissions(appId);

        // add
        foreach (var obValue in obValues.Where(x => dbValues.All(c => c.AppId == appId && x.PermissionId != c.PermissionId)))
            await _authRepo.AddEntity(new PermissionModel
            {
                AppId = appId,
                PermissionId = obValue.PermissionId,
                PermissionName = obValue.PermissionName
            });

        // delete
        var deleteValues = dbValues.Where(x => obValues.All(c => x.AppId == appId && x.PermissionId != c.PermissionId));
        var permissionModels = deleteValues as PermissionModel[] ?? deleteValues.ToArray();
        if (permissionModels.Any())
            _authRepo.RemoveEntities(permissionModels);

        // update
        foreach (var dbValue in dbValues)
        {
            var obValue = obValues.SingleOrDefault(x => x.PermissionId == dbValue.PermissionId);
            if (obValue == null) continue;
            dbValue.PermissionName = obValue.PermissionName;
        }
    }

    private static void UpdatePermissionGroupPermissions(ICollection<PermissionGroupPermissionModel> dbValues, PermissionGroupPermissionModel[] obValues)
    {
        // add
        foreach (var obValue in obValues.
                     Where(x => dbValues.All(c => x.PermissionId != c.PermissionId))
                )
            dbValues.Add(obValue);

        // delete
        foreach (var dbValue in dbValues.Where(x => obValues.All(c => x.PermissionId != c.PermissionId)))
            dbValues.Remove(dbValue);
    }

    public async Task UpdatePermissionGroups(int appId, PermissionGroup[] obValues, bool removeOthers)
    {
        var permissionGroups = await _authRepo.GetPermissionGroups(appId);

        // add
        foreach (var obValue in obValues.Where(x => permissionGroups.All(c => x.PermissionGroupId != c.PermissionGroupExternalId)))
        {
            var permissionGroup = new PermissionGroupModel
            {
                AppId = appId,
                PermissionGroupExternalId = obValue.PermissionGroupId,
                PermissionGroupName = obValue.PermissionGroupName
            };
            await _authRepo.AddEntity(permissionGroup);

            UpdatePermissionGroupPermissions(permissionGroup.PermissionGroupPermissions,
                obValue.Permissions.Select(x => new PermissionGroupPermissionModel { AppId = appId, PermissionGroupId = permissionGroup.PermissionGroupId, PermissionId = x.PermissionId }).ToArray());
        }

        // delete
        if (removeOthers)
        {
            foreach (var permissionGroup in permissionGroups.Where(x => obValues.All(c => x.PermissionGroupExternalId != c.PermissionGroupId)))
            {
                // Remove PermissionGroupPermission
                await RemovePermissionGroupPermission(appId, permissionGroup.PermissionGroupId);

                // Remove PermissionGroup
                _authRepo.RemoveEntity(permissionGroup);
            }
        }

        // update
        foreach (var dbValue in permissionGroups)
        {
            var obValue = obValues.SingleOrDefault(x => x.PermissionGroupId == dbValue.PermissionGroupExternalId);
            if (obValue == null) continue;

            UpdatePermissionGroupPermissions(dbValue.PermissionGroupPermissions,
                obValue.Permissions.Select(x => new PermissionGroupPermissionModel { AppId = appId, PermissionGroupId = dbValue.PermissionGroupId, PermissionId = x.PermissionId }).ToArray());

            dbValue.PermissionGroupName = obValue.PermissionGroupName;
        }
    }
}