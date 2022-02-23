namespace MultiLevelAuthorization.DTOs;

public class SecureObjectTypeDto
{
    public string SecureObjectTypeName { get; set; }

    public Guid SecureObjectTypeId { get; set; }

    public SecureObjectTypeDto(Guid secureObjectTypeId, string secureObjectTypeName)
    {
        SecureObjectTypeId = secureObjectTypeId;
        SecureObjectTypeName = secureObjectTypeName;
    }
}