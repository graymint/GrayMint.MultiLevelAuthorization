namespace GrayMint.MultiLevelAuthorization.Models;

public class RoleModel
{
    public Guid RoleId { get; set; }
    public int AppId { get; init; }
    public Guid OwnerId { get; set; }
    public string RoleName { get; set; } = null!;
    public Guid ModifiedByUserId { get; set; }
    public DateTime CreatedTime { get; set; }

    public virtual ICollection<SecureObjectRolePermissionModel>? RolePermissions { get; set; }
    public virtual ICollection<RoleUserModel>? RoleUsers { get; set; }

}