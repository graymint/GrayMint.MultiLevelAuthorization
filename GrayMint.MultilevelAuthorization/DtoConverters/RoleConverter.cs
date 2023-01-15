using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.DtoConverters;

public static class RoleConverter
{
    public static Role ToDto(this RoleModel roleModel)
    {
        return new Role
        {
            RoleId = roleModel.RoleId,
            RoleName = roleModel.RoleName,
            ModifiedByUserId = roleModel.ModifiedByUserId,
            OwnerId = roleModel.OwnerId
        };
    }
}