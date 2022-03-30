using System;
using System.Collections.Generic;
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test;
public static class PermissionGroups
{
    public static Apis.PermissionGroupDto UserBasic { get; } = new()
    {
        PermissionGroupId = Guid.NewGuid(),
        PermissionGroupName = nameof(UserBasic),
        Permissions = new List<PermissionDto>
        {
            Permissions.ProjectCreate,
            Permissions.ProjectList,
            Permissions.UserRead
        }
    };

    public static PermissionGroupDto ProjectViewer { get; } = new()
    {
        PermissionGroupId = Guid.NewGuid(), 
        PermissionGroupName = nameof(ProjectViewer),
        Permissions = new List<PermissionDto>
        {
            Permissions.UserRead,
            Permissions.ProjectRead,
        }
    };

    public static PermissionGroupDto ProjectOwner { get; } = new()
    {
        PermissionGroupId = Guid.NewGuid(),
        PermissionGroupName = nameof(ProjectOwner),
        Permissions = new List<PermissionDto>
        {
            Permissions.ProjectRead,
            Permissions.ProjectWrite,
        }
    };

    public static PermissionGroupDto[] All { get; } =
    {
        ProjectOwner,
        ProjectViewer,
        UserBasic
    };
}