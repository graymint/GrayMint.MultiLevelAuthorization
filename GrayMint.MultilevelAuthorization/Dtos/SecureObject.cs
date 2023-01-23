namespace MultiLevelAuthorization.Dtos;

public class SecureObject
{
    public required string SecureObjectTypeId { get; init; }
    public required string SecureObjectId { get; init; }
    public string? ParentSecureObjectTypeId { get; set; }
    public string? ParentSecureObjectId { get; set; }
}