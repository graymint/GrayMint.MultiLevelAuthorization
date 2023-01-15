using System;
using System.Collections.Generic;
using MultiLevelAuthorization.Test.Api;

namespace MultiLevelAuthorization.Test.Helper;
public static class PermissionGroups
{
    public static PermissionGroup UserBasic { get; } = new()
    {
        PermissionGroupId = Guid.NewGuid(),
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
        PermissionGroupId = Guid.NewGuid(), 
        PermissionGroupName = nameof(ProjectViewer),
        Permissions = new List<Permission>
        {
            Permissions.UserRead,
            Permissions.ProjectRead,
        }
    };

    public static PermissionGroup ProjectOwner { get; } = new()
    {
        PermissionGroupId = Guid.NewGuid(),
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