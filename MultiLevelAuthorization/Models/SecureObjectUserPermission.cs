using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class SecureObjectUserPermission
{
    public int AppId { get; set; }
    public int SecureObjectId { get; set; }
    public Guid UserId { get; set; }
    public int PermissionGroupId { get; set; }
    public Guid ModifiedByUserId { get; set; }
    public DateTime CreatedTime { get; set; }

    [JsonIgnore] public virtual SecureObject? SecureObject { get; set; }
    [JsonIgnore] public virtual PermissionGroup? PermissionGroup { get; set; }

}