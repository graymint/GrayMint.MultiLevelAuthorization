using MultiLevelAuthorization.Test.Api;

namespace MultiLevelAuthorization.Test.Helper;
public static class Permissions
{
    public static Permission ProjectCreate { get; } = new() { PermissionId = 10, PermissionName = nameof(ProjectCreate) };
    public static Permission ProjectRead { get; } = new() { PermissionId = 11, PermissionName = nameof(ProjectRead) };
    public static Permission ProjectWrite { get; } = new() { PermissionId = 12, PermissionName = nameof(ProjectWrite) };
    public static Permission ProjectList { get; } = new() { PermissionId = 13, PermissionName = nameof(ProjectList) };
    public static Permission UserRead { get; set; } = new() { PermissionId = 40, PermissionName = nameof(UserRead) };
    public static Permission UserWrite { get; set; } = new() { PermissionId = 41, PermissionName = nameof(UserWrite) };

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