namespace MultiLevelAuthorization.Models;

public class SecureObjectType
{
    public int SecureObjectTypeId { get; set; }
    public int AppId { get; set; }
    public Guid SecureObjectTypeExternalId { get; set; }
    public string SecureObjectTypeName { get; set; }

    public SecureObjectType(int appId, Guid secureObjectTypeExternalId, string secureObjectTypeName)
    {
        AppId = appId;
        SecureObjectTypeExternalId = secureObjectTypeExternalId;
        SecureObjectTypeName = secureObjectTypeName;
    }
    public App? App { get; set; }
    public bool IsSystem { get; set; }
    public virtual ICollection<SecureObject>? SecureObjects { get; set; }

}