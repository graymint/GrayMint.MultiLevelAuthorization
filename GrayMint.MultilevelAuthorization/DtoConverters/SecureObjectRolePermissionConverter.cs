using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.DtoConverters;

public static class SecureObjectRolePermissionConverter
{
    public static PermissionGroup ToDto(this SecureObjectRolePermissionModel model)
    {
        if (model.PermissionGroup == null) throw new Exception("PermissionGroups has not been included");
        if (model.PermissionGroup.Permissions == null) throw new Exception("Permissions has not been included");

        return new PermissionGroup
        {
            PermissionGroupId = model.PermissionGroup.PermissionGroupExternalId,
            PermissionGroupName = model.PermissionGroup.PermissionGroupName,
            Permissions = model.PermissionGroup.Permissions.Select(per => per.ToDto()).ToArray(),
        };
    }
}