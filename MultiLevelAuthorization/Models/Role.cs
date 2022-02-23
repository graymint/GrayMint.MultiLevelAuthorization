using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class Role
{
    public Guid AppId { get; init; }
    public Guid OwnerId { get; set; }
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = null!;
    public Guid ModifiedByUserId { get; set; }
    public DateTime CreatedTime { get; set; }

    public virtual AuthApp? App { get; set; }
    [JsonIgnore] public virtual ICollection<SecureObjectRolePermission>? RolePermissions { get; set; }

}