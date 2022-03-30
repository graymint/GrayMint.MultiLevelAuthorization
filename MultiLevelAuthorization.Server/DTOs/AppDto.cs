namespace MultiLevelAuthorization.Server.DTOs;

public class AppDto : MultiLevelAuthorization.DTOs.AppDto
{
    public Guid AppId { get; }
    public string AppName { get; }

    public AppDto(Guid appId, string appName, Guid systemSecureObjectId) 
        : base(systemSecureObjectId)
    {
        AppName = appName;
        AppId = appId;
    }
}