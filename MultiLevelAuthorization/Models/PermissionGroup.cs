using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class PermissionGroup
{
    public Guid AppId { get; set; }
    public Guid PermissionGroupId { get; set; }
    public string PermissionGroupName { get; set; }
    
    public virtual ICollection<Permission> Permissions { get; set; } = new HashSet<Permission>();
    public virtual AuthApp? App { get; set; }

    [JsonIgnore] public virtual ICollection<PermissionGroupPermission> PermissionGroupPermissions { get; set; } = new HashSet<PermissionGroupPermission>();
    [JsonIgnore] public virtual ICollection<SecureObjectRolePermission>? RolePermissions { get; set; }
    [JsonIgnore] public virtual ICollection<SecureObjectUserPermission>? UserPermissions { get; set; }

    public PermissionGroup(Guid permissionGroupId, string permissionGroupName)
    {
        PermissionGroupId = permissionGroupId;
        PermissionGroupName = permissionGroupName;
    }
}