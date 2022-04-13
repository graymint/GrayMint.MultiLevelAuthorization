using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Server.Controllers
{
    [ApiController]
    [Route("/api/apps")]
    public class RoleController : Controller
    {
        private readonly AuthManager _authManager;

        public RoleController(AuthManager authManager)
        {
            _authManager = authManager; 
        }
        [HttpPost("{appId}/role")]
        public async Task<Role> Role_Create(string appId, string roleName, Guid ownerId, Guid modifiedByUserId)
        {
            //todo: check permission

            var result = await _authManager.Role_Create(appId, roleName, ownerId, modifiedByUserId);
            return result;
        }

        [HttpGet("{appId}/roles")]
        public async Task<Role[]> Role_List(string appId)
        {
            //todo: check permission

            var result = await _authManager.Role_List(appId);

            return result;
        }
        [HttpGet("{appId}/role-users")]
        public async Task<RoleUser[]> Role_Users(string appId, Guid roleId)
        {
            //todo: check permission

            var result = await _authManager.Role_Users(appId, roleId);

            return result;
        }
    }
}
