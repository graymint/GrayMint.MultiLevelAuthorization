namespace MultiLevelAuthorization.DTOs;

public class PermissionDto
{
    public int PermissionCode { get; set; }
    public string PermissionName { get; set; }

    public PermissionDto(int permissionCode, string permissionName)
    {
        PermissionCode = permissionCode;
        PermissionName = permissionName;
    }
}