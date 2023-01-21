// ReSharper disable UnusedMember.Global
namespace MultiLevelAuthorization.Models;

public class AppModel
{
    public int AppId { get; set; }
    public int AuthorizationCode { get; set; }

    public virtual ICollection<SecureObjectTypeModel>? SecureObjectTypes { get; set; }
    public virtual ICollection<PermissionGroupModel>? PermissionGroups { get; set; }
    public virtual ICollection<PermissionGroupPermissionModel>? GroupPermissions { get; set; }
    public virtual ICollection<PermissionModel>? Permissions { get; set; }
    public virtual ICollection<RoleModel>? Roles { get; set; }
}