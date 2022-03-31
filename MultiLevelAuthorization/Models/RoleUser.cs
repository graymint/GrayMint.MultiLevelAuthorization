using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class RoleUser
{
    public int AppId { get; set; }
    public Guid RoleId { get; set; }
    public Guid UserId { get; set; } 
    public Guid ModifiedByUserId { get; set; }
    public DateTime CreatedTime { get; set; }

    public virtual Role? Role { get; set; }
}