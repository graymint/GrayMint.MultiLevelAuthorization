using MultiLevelAuthorization.DTOs;

namespace MultiLevelAuthorization.Server.DTOs;

public class AppInitRequest
{
    public AppInitRequest(Guid rootSecureObjectId, SecureObjectTypeDto[] secureObjectTypes, PermissionDto[] permissions, PermissionGroupDto[] permissionGroups, bool removeOtherPermissionGroups = true)
    {
        RootSecureObjectId = rootSecureObjectId;
        SecureObjectTypes = secureObjectTypes;
        Permissions = permissions;
        PermissionGroups = permissionGroups;
        RemoveOtherPermissionGroups = removeOtherPermissionGroups;
    }

    public Guid RootSecureObjectId { get; set; }
    public SecureObjectTypeDto[] SecureObjectTypes { get; set; }
    public PermissionDto[] Permissions { get; }
    public PermissionGroupDto[] PermissionGroups { get; }
    public bool RemoveOtherPermissionGroups { get; }
}