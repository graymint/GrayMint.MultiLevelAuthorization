namespace GrayMint.MultiLevelAuthorization.Models;

public class RoleUserModel
{
    public Guid RoleId { get; set; }
    public int AppId { get; set; }
    public Guid UserId { get; set; } 
    public Guid ModifiedByUserId { get; set; }
    public DateTime CreatedTime { get; set; }

    public virtual RoleModel? Role { get; set; }
}