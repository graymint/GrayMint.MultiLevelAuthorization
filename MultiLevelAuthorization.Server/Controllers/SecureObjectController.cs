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

    }
}
