namespace MultiLevelAuthorization.Services.Views;

public class RoleView
{
    public required Guid RoleId { get; init; }
    public required Guid ModifiedByUserId { get; init; }
    public required Guid OwnerId { get; init; }
    public required string RoleName { get; init; }
}