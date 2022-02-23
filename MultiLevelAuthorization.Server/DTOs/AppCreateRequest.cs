namespace MultiLevelAuthorization.Server.DTOs;

public class AppCreateRequest
{
    public AppCreateRequest(string appName)
    {
        AppName = appName;
    }

    public string AppName { get; set; }
}