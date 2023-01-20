namespace MultiLevelAuthorization.Dtos;

public class SecureObject
{
    public required string SecureObjectId { get; init; }
    public required string SecureObjectTypeId { get; init; }
    public string? ParentSecureObjectId { get; set; }
}