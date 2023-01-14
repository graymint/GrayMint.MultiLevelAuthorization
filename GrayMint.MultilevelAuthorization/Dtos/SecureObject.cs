namespace GrayMint.MultiLevelAuthorization.Dtos;

public class SecureObject
{
    public required Guid SecureObjectId { get; init; }
    public required Guid SecureObjectTypeId { get; init; }
    public Guid? ParentSecureObjectId { get; set; }
}