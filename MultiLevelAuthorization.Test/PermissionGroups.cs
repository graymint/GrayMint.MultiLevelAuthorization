using System;
using System.Collections.Generic;
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test;
public static class PermissionGroups
{
    public static PermissionGroup UserBasic { get; } = new()
    {
        PermissionGroupId = Guid.Parse("{44F09001-3C94-48D7-A2AC-88CEF10B27E3}"),
        PermissionGroupName = nameof(UserBasic),
        Permissions = new List<Permission>
        {
            Permissions.ProjectCreate,
            Permissions.ProjectList,
            Permissions.UserRead
        }
    };

    public static PermissionGroup ProjectViewer { get; } = new()
    {
        PermissionGroupId = Guid.Parse("{043310B2-752F-4087-BC1B-E63B431A45FA}"), 
        PermissionGroupName = nameof(ProjectViewer),
        Permissions = new List<Permission>
        {
            Permissions.UserRead,
            Permissions.ProjectRead,
        }
    };

    public static PermissionGroup ProjectOwner { get; } = new()
    {
        PermissionGroupId = Guid.Parse("{90DD2AC7-7F41-4E07-8977-FBC3C610AEAE}"),
        PermissionGroupName = nameof(ProjectOwner),
        Permissions = new List<Permission>
        {
            Permissions.ProjectRead,
            Permissions.ProjectWrite,
        }
    };

    public static PermissionGroup[] All { get; } =
    {
        ProjectOwner,
        ProjectViewer,
        UserBasic
    };
}