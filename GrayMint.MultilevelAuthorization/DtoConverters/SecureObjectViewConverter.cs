using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Services.Views;

namespace MultiLevelAuthorization.DtoConverters;

public static class SecureObjectViewConverter
{
    public static SecureObject ToDto(this SecureObjectView view)
    {
        return new SecureObject
        {
            SecureObjectTypeId = view.SecureObjectTypeId,
            ParentSecureObjectId = view.ParentSecureObjectId,
            SecureObjectId = view.SecureObjectId
        };
    }
}