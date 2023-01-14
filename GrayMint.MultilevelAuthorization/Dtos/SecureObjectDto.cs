namespace MultiLevelAuthorization.DTOs;

public class SecureObjectDto
{
    public Guid SecureObjectId { get; set; }
    public Guid SecureObjectTypeId { get; set; }
    public Guid? ParentSecureObjectId { get; set; }

    public SecureObjectDto(Guid secureObjectId, Guid secureObjectTypeId, Guid? parentSecureObjectId)
    {
        SecureObjectId = secureObjectId;
        SecureObjectTypeId = secureObjectTypeId;
        ParentSecureObjectId = parentSecureObjectId;
    }

}