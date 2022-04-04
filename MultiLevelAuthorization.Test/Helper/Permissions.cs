
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test.Helper;
public static class Permissions
{
    public static PermissionDto ProjectCreate { get; } = new PermissionDto { PermissionCode = 10, PermissionName = nameof(ProjectCreate) };
    public static PermissionDto ProjectRead { get; } = new PermissionDto { PermissionCode = 11, PermissionName = nameof(ProjectRead) };
    public static PermissionDto ProjectWrite { get; } = new PermissionDto { PermissionCode = 12, PermissionName = nameof(ProjectWrite) };
    public static PermissionDto ProjectList { get; } = new PermissionDto { PermissionCode = 13, PermissionName = nameof(ProjectList) };
    public static PermissionDto UserRead { get; set; } = new PermissionDto { PermissionCode = 40, PermissionName = nameof(UserRead) };
    public static PermissionDto UserWrite { get; set; } = new PermissionDto { PermissionCode = 41, PermissionName = nameof(UserWrite) };

    public static PermissionDto[] All { get; } =
    {
        ProjectCreate,
        ProjectRead,
        ProjectWrite,
        ProjectList,
        UserRead,
        UserWrite,
    };

}