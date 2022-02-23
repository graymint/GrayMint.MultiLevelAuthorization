using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class Permission
{
    public Guid AppId { get; set; }
    public Guid PermissionId { get; set; }
    public int PermissionCode { get; set; }
    public string PermissionName { get; set; }

    public Permission(Guid appId, Guid permissionId, int permissionCode, string permissionName)
    {
        AppId = appId;
        PermissionId = permissionId;
        PermissionCode = permissionCode;
        PermissionName = permissionName;
    }

    public AuthApp? App { get; set; }
   
    public ICollection<PermissionGroup>? PermissionGroups { get; set; }
    public ICollection<PermissionGroupPermission>? PermissionGroupPermissions { get; set; }
}