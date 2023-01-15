using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.DtoConverters;

public static class SecureObjectTypeConverter
{
    public static SecureObjectType ToDto(this SecureObjectTypeModel model)
    {
        return new SecureObjectType
        {
            SecureObjectTypeId = model.SecureObjectTypeExternalId,
            SecureObjectTypeName = model.SecureObjectTypeName
        };
    }
}