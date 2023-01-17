namespace MultiLevelAuthorization.Services.Views;

public class SecureObjectView
{
    public Guid SecureObjectExternalId { get; set; }
    public Guid SecureObjectTypeId { get; set; }
    public Guid? ParentSecureObjectExternalId { get; set; }
}