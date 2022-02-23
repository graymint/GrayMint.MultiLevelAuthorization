
using MultiLevelAuthorization.Models;

namespace MultiLevelAuthorization.Server.Models;

public class App : AuthApp
{
    public string AppName { get; set; } = default!;
}