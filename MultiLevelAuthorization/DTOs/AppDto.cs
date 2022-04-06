namespace MultiLevelAuthorization.DTOs;

public class AppDto
{
    public AppDto(string appName, string appDescription, Guid systemSecureObjectId)
    {
        AppName = appName;
        AppDescription = appDescription;
        SystemSecureObjectId = systemSecureObjectId;
    }
    public string AppName { get; }
    public string AppDescription { get; }
    public Guid SystemSecureObjectId { get; }
}
