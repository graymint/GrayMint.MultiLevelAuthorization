namespace MultiLevelAuthorization.Models;

public class SecureObjectType
{
    public short AppId { get; set; }
    public Guid SecureObjectTypeId { get; set; }
    public string SecureObjectTypeName { get; set; }

    public SecureObjectType(short appId, Guid secureObjectTypeId, string secureObjectTypeName)
    {
        AppId = appId;
        SecureObjectTypeId = secureObjectTypeId;
        SecureObjectTypeName = secureObjectTypeName;
    }
    public App? App { get; set; }
    public bool IsSystem { get; set; }
    public virtual ICollection<SecureObject>? SecureObjects { get; set; }

}