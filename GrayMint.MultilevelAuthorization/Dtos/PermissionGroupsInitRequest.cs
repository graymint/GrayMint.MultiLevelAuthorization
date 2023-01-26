namespace MultiLevelAuthorization.Dtos;

public class PermissionGroupsInitRequest
{
    public required Guid PermissionGroupId { get; init; }
    public required string PermissionGroupName { get; init; }
    public required int[] Permissions { get; init; }
}