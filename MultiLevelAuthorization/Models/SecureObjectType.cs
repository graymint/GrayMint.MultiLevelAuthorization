using System.Text.Json.Serialization;

namespace MultiLevelAuthorization.Models;

public class SecureObjectType
{
    public SecureObjectType(Guid appId, Guid secureObjectTypeId, string secureObjectTypeName)
    {
        AppId = appId;
        SecureObjectTypeId = secureObjectTypeId;
        SecureObjectTypeName = secureObjectTypeName;
    }

    public Guid AppId { get; set; }
    public Guid SecureObjectTypeId { get; set; }
    public string SecureObjectTypeName { get; set; }

    public AuthApp? App { get; set; }
    [JsonIgnore] public bool IsSystem { get; set; }
    [JsonIgnore] public virtual ICollection<SecureObject>? SecureObjects { get; set; }
}