namespace MultiLevelAuthorization.Services.Views;

public class SecureObjectView
{
    public Guid SecureObjectId { get; set; }
    public Guid SecureObjectTypeId { get; set; }
    public Guid? ParentSecureObjectId { get; set; }
}