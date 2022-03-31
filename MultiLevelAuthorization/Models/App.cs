using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class App
{
    public int AppId { get; set; }
    public Guid AppGuid { get; set; }
    public string AppName { get; set; } = default!;

    public virtual ICollection<SecureObjectType>? SecureObjectTypes { get; set; }
    public virtual ICollection<PermissionGroup>? PermissionGroups { get; set; }
    public virtual ICollection<PermissionGroupPermission>? GroupPermissions { get; set; }
    public virtual ICollection<Permission>? Permissions { get; set; }
    public virtual ICollection<Role>? Roles { get; set; }
}