using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class SecureObjectUserPermission
{
    public short AppId { get; set; }
    public Guid SecureObjectId { get; set; }
    public Guid UserId { get; set; }
    public Guid PermissionGroupId { get; set; }
    public Guid ModifiedByUserId { get; set; }
    public DateTime CreatedTime { get; set; }
        
    [JsonIgnore] public virtual SecureObject? SecureObject { get; set; }
    [JsonIgnore] public virtual PermissionGroup? PermissionGroup { get; set; }

}