namespace MultiLevelAuthorization.Dtos;

public class SecureObjectType
{
    public required string SecureObjectTypeName { get; init; }
    public required Guid SecureObjectTypeId { get; init; }
}