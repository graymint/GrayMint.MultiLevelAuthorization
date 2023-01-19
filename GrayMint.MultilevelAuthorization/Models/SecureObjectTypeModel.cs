// ReSharper disable UnusedMember.Global
namespace MultiLevelAuthorization.Models;

public class SecureObjectTypeModel
{
    public int SecureObjectTypeId { get; set; }
    public int AppId { get; set; }
    public Guid SecureObjectTypeExternalId { get; set; }
    public string SecureObjectTypeName { get; set; } = default!;

    public AppModel? App { get; set; }
    public bool IsSystem { get; set; }
    public virtual ICollection<SecureObjectModel>? SecureObjects { get; set; }

}