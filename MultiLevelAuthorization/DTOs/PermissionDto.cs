namespace MultiLevelAuthorization.DTOs;

public class PermissionDto
{
    public int PermissionId { get; set; }
    public string PermissionName { get; set; }

    public PermissionDto(int permissionId, string permissionName)
    {
        PermissionId = permissionId;
        PermissionName = permissionName;
    }
}