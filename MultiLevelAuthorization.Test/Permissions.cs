
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test;

public static class Permissions
{
    public static Permission ProjectCreate { get; } = new Permission { PermissionId = 10, PermissionName = nameof(ProjectCreate) };
    public static Permission ProjectRead { get; } = new Permission { PermissionId = 11, PermissionName = nameof(ProjectRead) };
    public static Permission ProjectWrite { get; } = new Permission { PermissionId = 12, PermissionName = nameof(ProjectWrite) };
    public static Permission ProjectList { get; } = new Permission { PermissionId = 13, PermissionName = nameof(ProjectList) };
    public static Permission UserRead { get; set; } = new Permission { PermissionId = 40, PermissionName = nameof(UserRead) };
    public static Permission UserWrite { get; set; } = new Permission { PermissionId = 41, PermissionName = nameof(UserWrite) };

    public static Permission[] All { get; } =
    {
        ProjectCreate,
        ProjectRead,
        ProjectWrite,
        ProjectList,
        UserRead,
        UserWrite,
    };

}