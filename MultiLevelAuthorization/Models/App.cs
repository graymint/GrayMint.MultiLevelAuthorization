using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class App
{
    public short AppId { get; set; }

    public virtual ICollection<SecureObjectType>? SecureObjectTypes { get; set; }
    public virtual ICollection<PermissionGroup>? PermissionGroups { get; set; }
    public virtual ICollection<PermissionGroupPermission>? GroupPermissions { get; set; }
    public virtual ICollection<Permission>? Permissions { get; set; }
    public virtual ICollection<Role>? Roles { get; set; }
}