namespace MultiLevelAuthorization.DTOs;

public class AppDto
{
    public AppDto(Guid systemSecureObjectId)
    {
        SystemSecureObjectId = systemSecureObjectId;
    }

    public Guid SystemSecureObjectId { get; }
}
