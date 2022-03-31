using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLevelAuthorization.DTOs;
public class AppCreateRequestHandler
{
    public AppCreateRequestHandler(string appName)
    {
        AppName = appName;
    }

    public string AppName { get; set; }
}
