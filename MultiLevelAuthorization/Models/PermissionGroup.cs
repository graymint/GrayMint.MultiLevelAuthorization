using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class PermissionGroup
{
    public int PermissionGroupId { get; set; }
    public int AppId { get; set; }
    public Guid PermissionGroupGuid { get; set; }
    public string PermissionGroupName { get; set; }

    public virtual ICollection<Permission> Permissions { get; set; } = new HashSet<Permission>();
    public virtual App? App { get; set; }

    [JsonIgnore] public virtual ICollection<PermissionGroupPermission> PermissionGroupPermissions { get; set; } = new HashSet<PermissionGroupPermission>();
    [JsonIgnore] public virtual ICollection<SecureObjectRolePermission>? RolePermissions { get; set; }
    [JsonIgnore] public virtual ICollection<SecureObjectUserPermission>? UserPermissions { get; set; }

    public PermissionGroup(int appId, Guid permissionGroupGuid, string permissionGroupName)
    {
        AppId = appId;
        PermissionGroupGuid = permissionGroupGuid;
        PermissionGroupName = permissionGroupName;
    }
}