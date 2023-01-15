namespace MultiLevelAuthorization.Dtos;

public class Role
{
    public required Guid RoleId { get; init; }
    public required Guid OwnerId { get; init; }
    public required Guid ModifiedByUserId { get; init; }
    public required string RoleName { get; init; }
}