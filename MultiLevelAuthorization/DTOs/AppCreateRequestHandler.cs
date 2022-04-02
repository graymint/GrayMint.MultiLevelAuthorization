using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLevelAuthorization.DTOs;
public class AppCreateRequestHandler
{
    public AppCreateRequestHandler(string appName, string appDescription)
    {
        AppName = appName;
        AppDescription = appDescription;
    }

    public string AppName { get; set; }
    public string AppDescription { get; set; }
}
