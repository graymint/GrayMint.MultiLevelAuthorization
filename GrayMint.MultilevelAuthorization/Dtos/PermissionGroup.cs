namespace MultiLevelAuthorization.Dtos;

public class PermissionGroup
{
    public required Guid PermissionGroupId { get; init; }
    public required string PermissionGroupName { get; init; }
    public required Permission[] Permissions { get; init; }
}