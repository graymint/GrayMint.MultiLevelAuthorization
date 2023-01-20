// ReSharper disable UnusedMember.Global
namespace MultiLevelAuthorization.Models;

public class RoleModel
{
    public Guid RoleId { get; set; }
    public int AppId { get; init; }
    public int OwnerSecureObjectId { get; set; }
    public string RoleName { get; set; } = null!;
    public Guid ModifiedByUserId { get; set; }
    public DateTime CreatedTime { get; set; }

    public virtual AppModel? App { get; set; }
    public virtual SecureObjectModel? OwnerSecureObject { get; set; }
    public virtual ICollection<SecureObjectRolePermissionModel>? RolePermissions { get; set; }
    public virtual ICollection<RoleUserModel>? RoleUsers { get; set; }
}