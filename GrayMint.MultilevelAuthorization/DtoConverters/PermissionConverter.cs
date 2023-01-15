using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.DtoConverters;

public static class PermissionConverter
{
    public static Permission ToDto(this PermissionModel model)
    {
        return new Permission
        {
            PermissionId = model.PermissionId,
            PermissionName = model.PermissionName
        };
    }
}