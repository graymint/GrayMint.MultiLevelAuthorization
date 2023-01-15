using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class PermissionGroupModel
{
    public int PermissionGroupId { get; set; }
    public int AppId { get; set; }
    public Guid PermissionGroupExternalId { get; set; }
    public string PermissionGroupName { get; set; } = default!;

    public virtual ICollection<PermissionModel> Permissions { get; set; } = new HashSet<PermissionModel>();
    public virtual AppModel? App { get; set; }

    [JsonIgnore] public virtual ICollection<PermissionGroupPermissionModel> PermissionGroupPermissions { get; set; } = new HashSet<PermissionGroupPermissionModel>();
    [JsonIgnore] public virtual ICollection<SecureObjectRolePermissionModel>? RolePermissions { get; set; }
    [JsonIgnore] public virtual ICollection<SecureObjectUserPermissionModel>? UserPermissions { get; set; }
}