using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.DtoConverters;

public static class RoleConverter
{
    public static Role ToDto(this RoleModel model)
    {
        if (model.OwnerSecureObject == null)
            throw new ArgumentException("OwnerSecureObject has not been included.");

        if (model.OwnerSecureObject.SecureObjectType == null)
            throw new ArgumentException("OwnerSecureObjectType has not been included.");

        return new Role
        {
            RoleId = model.RoleId,
            RoleName = model.RoleName,
            ModifiedByUserId = model.ModifiedByUserId,
            OwnerSecureObjectId = model.OwnerSecureObject.SecureObjectExternalId,
            OwnerSecureObjectTypeId = model.OwnerSecureObject.SecureObjectType.SecureObjectTypeExternalId,
            Users = model.RoleUsers?.Select(roleUser => new User { UserId = roleUser.UserId }).ToArray()
        };
    }
}