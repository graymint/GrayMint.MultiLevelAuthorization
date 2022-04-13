using Microsoft.AspNetCore.Mvc;
using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Server.Controllers
{
    [ApiController]
    [Route("/api/apps")]

    public class SecureObjectController : Controller
    {
        private readonly AuthManager _authManager;

        public SecureObjectController(AuthManager authManager)
        {
            _authManager = authManager;
        }
        [HttpGet("{appId}/secureObjectTypes")]
        public async Task<List<SecureObjectTypeDto>> SecureObjectType_List(string appId)
        {
            //todo: check permission

            var result = await _authManager.SecureObjectType_List(appId);
            List<SecureObjectTypeDto> list = new List<SecureObjectTypeDto>();
            foreach (var item in result)
            {
                list.Add(new SecureObjectTypeDto(item.SecureObjectTypeExternalId, item.SecureObjectTypeName));
            }
            return list;
        }

        [HttpGet("{appId}/secureObjects")]
        public async Task<List<SecureObjectDto>> SecureObject_List(string appId)
        {
            //todo: check permission

            var result = await _authManager.SecureObject_List(appId);
            return result;
        }

        [HttpPost("{appId}/secureObject")]
        public async Task<SecureObjectDto> SecureObject_Create(string appId, Guid secureObjectId, Guid secureObjectTypeId, Guid? parentSecureObjectId)
        {
            //todo: check permission

            var result = await _authManager.SecureObject_Create(appId, secureObjectId, secureObjectTypeId, parentSecureObjectId);
            return result;
        }

        [HttpPost("{appId}/secureObject-adduserpermission")]
        public async Task<IActionResult> SecureObject_AddUserPermission(string appId, Guid secureObjectId, Guid userId, Guid permissionGroupId,
            Guid modifiedByUserId)
        {
            //todo: check permission

            await _authManager.SecureObject_AddUserPermission(appId, secureObjectId, userId, permissionGroupId, modifiedByUserId);
            return Ok();
        }

        [HttpPost("{appId}/secureObject-addrolepermission")]
        public async Task<IActionResult> SecureObject_AddRolePermission(string appId, Guid secureObjectId, Guid roleId, Guid permissionGroupId, Guid modifiedByUserId)
        {
            //todo: check permission

            await _authManager.SecureObject_AddRolePermission(appId, secureObjectId, roleId, permissionGroupId, modifiedByUserId);
            return Ok();
        }

        [HttpGet("{appId}/secureObject-userpermissions")]
        public async Task<List<PermissionDto>> SecureObject_GetUserPermissions(string appId, Guid secureObjectId, Guid userId)
        {
            //todo: check permission

            var result = await _authManager.SecureObject_GetUserPermissions(appId, secureObjectId, userId);
            return result;
        }
    }
}
