
namespace MultiLevelAuthorization.Models;

public class SecureObjectModel
{
    public int SecureObjectId { get; set; }
    public int AppId { get; set; }
    public int SecureObjectTypeId { get; set; }
    public int? ParentSecureObjectId { get; set; }
    public string SecureObjectExternalId { get; set; } = default!;

    public virtual SecureObjectModel? ParentSecureObject { get; set; }
    public virtual SecureObjectTypeModel? SecureObjectType { get; set; }
    public virtual ICollection<SecureObjectRolePermissionModel>? RolePermissions { get; set; }
    public virtual ICollection<SecureObjectUserPermissionModel>? UserPermissions { get; set; }

}