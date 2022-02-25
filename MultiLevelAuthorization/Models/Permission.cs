using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class Permission
{
    public short AppId { get; set; }
    public int PermissionId { get; set; }
    public string PermissionName { get; set; }

    public Permission(short appId, int permissionId, string permissionName)
    {
        AppId = appId;
        PermissionId = permissionId;
        PermissionName = permissionName;
    }

    public App? App { get; set; }
    public ICollection<PermissionGroup>? PermissionGroups { get; set; }
    public ICollection<PermissionGroupPermission>? PermissionGroupPermissions { get; set; }
}