namespace MultiLevelAuthorization.Server.DTOs;

public class AppDto : MultiLevelAuthorization.DTOs.AppDto
{
    public string AppName { get; }

    public AppDto(Guid systemSecureObjectId, string appName) 
        : base(systemSecureObjectId)
    {
        AppName = appName;
    }
}