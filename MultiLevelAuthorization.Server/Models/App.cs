namespace MultiLevelAuthorization.Server.Models;

public class App 
{
    public short AppId { get; set; }
    public Guid AppGuid { get; set; }
    public string AppName { get; set; } = default!;
}
