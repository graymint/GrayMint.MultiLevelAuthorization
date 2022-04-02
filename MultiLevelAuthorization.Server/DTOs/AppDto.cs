namespace MultiLevelAuthorization.Server.DTOs;

public class AppDto : MultiLevelAuthorization.DTOs.AppDto
{
    public string AppName { get; }
    public string AppDescription { get; }

    public AppDto(string appName, string AppDescription, Guid systemSecureObjectId)
        : base(systemSecureObjectId)
    {
        AppName = appName;
        AppDescription = AppDescription;
    }
}