using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLevelAuthorization.Repositories
{
    public interface IAuthManager
    {
        Task<AppDto> Init(int appId, SecureObjectTypeDto[] secureObjectTypes, PermissionDto[] permissions, PermissionGroupDto[] permissionGroups, bool removeOtherPermissionGroups = true);

        Task<PermissionGroup[]> GetPermissionGroups(int appId);
        Task<SecureObject> CreateSecureObject(int appId, Guid secureObjectId, Guid secureObjectTypeId);
        Task<SecureObject> CreateSecureObject(int appId, Guid secureObjectId, Guid secureObjectTypeId, Guid parentSecureObjectId);
        Task<Role> Role_Create(int appId, string roleName, Guid ownerId, Guid modifiedByUserId);
        Task Role_AddUser(int appId, Role role, Guid userId, Guid modifiedByUserId);
        Task Role_AddUser(int appId, Guid roleId, Guid userId, Guid modifiedByUserId);
        Task<SecureObjectRolePermission> SecureObject_AddRolePermission(int appId, Guid secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId);
        Task<SecureObjectUserPermission> SecureObject_AddUserPermission(int appId, Guid secureObjectId, Guid userId, Guid permissionGroupId, Guid modifiedByUserId);
        Task<SecureObjectRolePermission[]> SecureObject_GetRolePermissionGroups(int appId, Guid secureObjectId);
        Task<SecureObjectUserPermission[]> SecureObject_GetUserPermissionGroups(int appId, Guid secureObjectId);
        Task<Permission[]> SecureObject_GetUserPermissions(int appId, Guid secureObjectId, Guid userId);
        Task<bool> SecureObject_HasUserPermission(int appId, Guid secureObjectId, Guid userId, Permission permission);
        Task<bool> SecureObject_HasUserPermission(int appId, Guid secureObjectId, Guid userId, int permissionId);
        Task SecureObject_VerifyUserPermission(int appId, Guid secureObjectId, Guid userId, Permission permission);
        Task<Guid> App_Create(AppCreateRequestHandler request);
    }
}
