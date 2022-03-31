namespace MultiLevelAuthorization.Models;

public class SecureObjectType
{
    public int AppId { get; set; }
    public Guid SecureObjectTypeId { get; set; }
    public string SecureObjectTypeName { get; set; }

    public SecureObjectType(int appId, Guid secureObjectTypeId, string secureObjectTypeName)
    {
        AppId = appId;
        SecureObjectTypeId = secureObjectTypeId;
        SecureObjectTypeName = secureObjectTypeName;
    }
    public App? App { get; set; }
    public bool IsSystem { get; set; }
    public virtual ICollection<SecureObject>? SecureObjects { get; set; }

}