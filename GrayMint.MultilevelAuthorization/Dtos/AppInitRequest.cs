namespace MultiLevelAuthorization.Dtos;

public class AppInitRequest
{
    public required SecureObjectType[] SecureObjectTypes { get; init; }
    public required Permission[] Permissions { get; init; }
    public required PermissionGroup[] PermissionGroups { get; init; }
    public required bool RemoveOtherPermissionGroups { get; init; }
}