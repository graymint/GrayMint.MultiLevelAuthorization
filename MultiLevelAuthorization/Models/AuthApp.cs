using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class AuthApp
{
    public Guid AppId { get; set; }

    [JsonIgnore] public virtual ICollection<SecureObjectType>? SecureObjectTypes { get; set; }
    [JsonIgnore] public virtual ICollection<PermissionGroup>? PermissionGroups { get; set; }
    [JsonIgnore] public virtual ICollection<Permission>? Permissions { get; set; }
    [JsonIgnore] public virtual ICollection<Role>? Roles { get; set; }
}