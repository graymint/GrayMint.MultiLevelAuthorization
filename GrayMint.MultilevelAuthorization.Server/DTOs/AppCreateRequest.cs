namespace MultiLevelAuthorization.Server.DTOs;

public class AppCreateRequest
{
    public AppCreateRequest(string appName, string appDescription)
    {
        AppName = appName;
        AppDescription = appDescription;
    }

    public string AppName { get; set; }
    public string AppDescription { get; set; }
}