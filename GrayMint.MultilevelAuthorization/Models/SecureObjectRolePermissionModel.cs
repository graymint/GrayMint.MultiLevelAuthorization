namespace MultiLevelAuthorization.Models;

public class SecureObjectRolePermissionModel
{
    public int AppId { get; set; }
    public int SecureObjectId { get; set; }
    public Guid RoleId { get; set; }
    public int PermissionGroupId { get; set; }
    public Guid ModifiedByUserId { get; set; }
    public DateTime CreatedTime { get; set; }

    public virtual SecureObjectModel? SecureObject { get; set; }
    public virtual RoleModel? Role { get; set; }
    public virtual PermissionGroupModel? PermissionGroup { get; set; }
}