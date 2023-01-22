using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.DtoConverters;

public static class SecureObjectConverter
{
    public static SecureObject ToDto(this SecureObjectModel model)
    {
        if (model.SecureObjectType == null)
            throw new ArgumentException("SecureObjectType has not been included.");

        if (model.ParentSecureObject == null)
            throw new ArgumentException("ParentSecureObject has not been included.");

        return new SecureObject
        {
            SecureObjectId = model.SecureObjectExternalId,
            SecureObjectTypeId = model.SecureObjectType.SecureObjectTypeExternalId,
            ParentSecureObjectId = model.ParentSecureObject.SecureObjectExternalId
        };
    }
}