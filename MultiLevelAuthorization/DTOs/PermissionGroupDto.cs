namespace MultiLevelAuthorization.DTOs;

public class PermissionGroupDto
{
    public int PermissionGroupId { get; set; }
    public Guid PermissionGroupGuid { get; set; }
    public string PermissionGroupName { get; set; }
    public PermissionDto[] Permissions { get; set; }

    public PermissionGroupDto(int permissionGroupId, Guid permissionGroupGuid, string permissionGroupName, PermissionDto[] permissions)
    {
        PermissionGroupId = permissionGroupId;
        PermissionGroupGuid = permissionGroupGuid;
        PermissionGroupName = permissionGroupName;
        Permissions = permissions;
    }
}