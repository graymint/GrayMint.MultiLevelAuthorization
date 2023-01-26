namespace MultiLevelAuthorization.Dtos;

public class Role
{
    public required Guid RoleId { get; init; }
    public required string OwnerSecureObjectTypeId { get; init; }
    public required string OwnerSecureObjectId { get; init; }
    public required Guid ModifiedByUserId { get; init; }
    public required string RoleName { get; init; }
    public User[]? Users { get; set; }
}