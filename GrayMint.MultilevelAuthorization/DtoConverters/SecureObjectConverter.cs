using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.DtoConverters;

public static class SecureObjectConverter
{
    public static SecureObject ToDto(this SecureObjectModel model)
    {
        if (model.SecureObjectType == null)
            throw new ArgumentException("SecureObjectType has not been included.");

        return new SecureObject
        {
            ParentSecureObjectId = model.ParentSecureObject?.SecureObjectExternalId,
            SecureObjectId = model.SecureObjectExternalId,
            SecureObjectTypeId = model.SecureObjectType.SecureObjectTypeExternalId
        };
    }
}