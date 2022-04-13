
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test.Helper;
public static class Permissions
{
    public static PermissionDto ProjectCreate { get; } = new PermissionDto { PermissionId = 10, PermissionName = nameof(ProjectCreate) };
    public static PermissionDto ProjectRead { get; } = new PermissionDto { PermissionId = 11, PermissionName = nameof(ProjectRead) };
    public static PermissionDto ProjectWrite { get; } = new PermissionDto { PermissionId = 12, PermissionName = nameof(ProjectWrite) };
    public static PermissionDto ProjectList { get; } = new PermissionDto { PermissionId = 13, PermissionName = nameof(ProjectList) };
    public static PermissionDto UserRead { get; set; } = new PermissionDto { PermissionId = 40, PermissionName = nameof(UserRead) };
    public static PermissionDto UserWrite { get; set; } = new PermissionDto { PermissionId = 41, PermissionName = nameof(UserWrite) };

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