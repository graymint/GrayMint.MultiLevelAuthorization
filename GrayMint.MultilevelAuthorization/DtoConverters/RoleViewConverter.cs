using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.DtoConverters;

public static class RoleUserConverter
{
    public static Role ToDto(this RoleUserModel model)
    {
        if (model.Role == null)
            throw new ArgumentException("Role has not been included.");

        if (model.Role.OwnerSecureObject == null)
            throw new ArgumentException("OwnerSecureObject has not been included.");

        return new Role
        {
            RoleId = model.RoleId,
            RoleName = model.Role.RoleName,
            ModifiedByUserId = model.Role.ModifiedByUserId,
            OwnerSecureObjectId = model.Role.OwnerSecureObject.SecureObjectExternalId
        };
    }
}