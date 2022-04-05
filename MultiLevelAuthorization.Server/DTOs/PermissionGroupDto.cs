
using MultiLevelAuthorization.DTOs;

namespace MultiLevelAuthorization.Server.DTOs;
public class PermissionGroupDto
{
    public Guid PermissionGroupId { get; set; }
    public string PermissionGroupName { get; set; }
    public PermissionDto[] Permissions { get; set; }

    public PermissionGroupDto(Guid permissionGroupId, string permissionGroupName, PermissionDto[] permissions)
    {
        PermissionGroupId = permissionGroupId;
        PermissionGroupName = permissionGroupName;
        Permissions = permissions;
    }
}
