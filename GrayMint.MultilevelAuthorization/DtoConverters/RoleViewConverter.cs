using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Services.Views;

namespace MultiLevelAuthorization.DtoConverters;

public static class RoleViewConverter
{
    public static Role ToDto(this RoleView view)
    {
        return new Role
        {
            RoleId = view.RoleId,
            RoleName = view.RoleName,
            ModifiedByUserId = view.ModifiedByUserId,
            OwnerId = view.OwnerId
        };
    }
}