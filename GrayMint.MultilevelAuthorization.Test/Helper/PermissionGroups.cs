using System;
using System.Collections.Generic;
using MultiLevelAuthorization.Test.Api;

namespace MultiLevelAuthorization.Test.Helper;
public static class PermissionGroups
{
    public static PermissionGroupsInitRequest UserBasic { get; } = new()
    {
        PermissionGroupId = Guid.NewGuid(),
        PermissionGroupName = nameof(UserBasic),
        Permissions = new List<int>
        {
            Permissions.ProjectCreate.PermissionId,
            Permissions.ProjectList.PermissionId,
            Permissions.UserRead.PermissionId
        }
    };

    public static PermissionGroupsInitRequest ProjectViewer { get; } = new()
    {
        PermissionGroupId = Guid.NewGuid(), 
        PermissionGroupName = nameof(ProjectViewer),
        Permissions = new List<int>
        {
            Permissions.UserRead.PermissionId,
            Permissions.ProjectRead.PermissionId,
        }
    };

    public static PermissionGroupsInitRequest ProjectOwner { get; } = new()
    {
        PermissionGroupId = Guid.NewGuid(),
        PermissionGroupName = nameof(ProjectOwner),
        Permissions = new List<int>
        {
            Permissions.ProjectRead.PermissionId,
            Permissions.ProjectWrite.PermissionId,
        }
    };

    public static PermissionGroupsInitRequest[] All { get; } =
    {
        ProjectOwner,
        ProjectViewer,
        UserBasic
    };
}