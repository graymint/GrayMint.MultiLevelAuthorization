namespace MultiLevelAuthorization.Server.Models;

public class App 
{
    public short AppId { get; set; } 
    public string AppName { get; set; }

    public App(short appId, string appName)
    {
        AppId = appId;
        AppName = appName;
    }
}
