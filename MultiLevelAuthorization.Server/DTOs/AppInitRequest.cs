using MultiLevelAuthorization.DTOs;

namespace MultiLevelAuthorization.Server.DTOs;

public class AppInitRequest
{
    public AppInitRequest(SecureObjectTypeDto[] secureObjectTypes, PermissionDto[] permissions, PermissionGroupDto[] permissionGroups, bool removeOtherPermissionGroups = true)
    {
        SecureObjectTypes = secureObjectTypes;
        Permissions = permissions;
        PermissionGroups = permissionGroups;
        RemoveOtherPermissionGroups = removeOtherPermissionGroups;
    }

    public SecureObjectTypeDto[] SecureObjectTypes { get; set; }
    public PermissionDto[] Permissions { get; }
    public PermissionGroupDto[] PermissionGroups { get; }
    public bool RemoveOtherPermissionGroups { get; }
}