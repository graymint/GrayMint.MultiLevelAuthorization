using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class SecureObjectRolePermission
{
    public int AppId { get; set; }
    public int SecureObjectId { get; set; }
    public Guid RoleId { get; set; }
    public int PermissionGroupId { get; set; }
    public Guid ModifiedByUserId { get; set; }
    public DateTime CreatedTime { get; set; }

    public virtual SecureObject? SecureObject { get; set; }
    public virtual Role? Role { get; set; }
    public virtual PermissionGroup? PermissionGroup { get; set; }
}