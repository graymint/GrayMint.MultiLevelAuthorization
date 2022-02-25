
namespace MultiLevelAuthorization.Models;

public class SecureObject
{
    public short AppId { get; set; }
    public Guid SecureObjectId { get; set; }
    public Guid SecureObjectTypeId { get; set; }
    public Guid? ParentSecureObjectId { get; set; }

    public virtual SecureObjectType? SecureObjectType { get; set; }
    public virtual ICollection<SecureObjectRolePermission>? RolePermissions { get; set; }
    public virtual ICollection<SecureObjectUserPermission>? UserPermissions { get; set; }

}