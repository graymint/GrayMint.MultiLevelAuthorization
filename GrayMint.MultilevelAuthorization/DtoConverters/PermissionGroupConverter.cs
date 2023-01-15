using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.DtoConverters;

public static class PermissionGroupConverter
{
    public static PermissionGroup ToDto(this PermissionGroupModel model)
    {
        return new PermissionGroup
        {
            PermissionGroupId = model.PermissionGroupExternalId,
            PermissionGroupName = model.PermissionGroupName,
            Permissions = model.Permissions
                .Select(x => new Permission
                {
                    PermissionId = x.PermissionId,
                    PermissionName = x.PermissionName
                })
                .ToArray()
        };
    }
}