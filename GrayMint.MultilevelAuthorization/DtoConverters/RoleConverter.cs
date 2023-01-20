using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.DtoConverters;

public static class RoleConverter
{
    public static Role ToDto(this RoleModel roleModel)
    {
        if (roleModel.OwnerSecureObject == null)
            throw new ArgumentException("OwnerSecureObject has not been included.");

        return new Role
        {
            RoleId = roleModel.RoleId,
            RoleName = roleModel.RoleName,
            ModifiedByUserId = roleModel.ModifiedByUserId,
            OwnerSecureObjectId = roleModel.OwnerSecureObject.SecureObjectExternalId
        };
    }
}