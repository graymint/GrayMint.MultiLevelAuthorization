
namespace MultiLevelAuthorization.Models;

public class SecureObject
{
    public int SecureObjectId { get; set; }
    public int AppId { get; set; }
    public int SecureObjectTypeId { get; set; }
    public int? ParentSecureObjectId { get; set; }
    public Guid SecureObjectExternalId { get; set; }

    public virtual SecureObjectType? SecureObjectType { get; set; }
    public virtual ICollection<SecureObjectRolePermission>? RolePermissions { get; set; }
    public virtual ICollection<SecureObjectUserPermission>? UserPermissions { get; set; }

}