using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MultiLevelAuthorization.DTOs;
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
        public async Task<RoleDto> Role_Create(string appId, string roleName, Guid ownerId, Guid modifiedByUserId)
        {
            //todo: check permission

            var result = await _authManager.Role_Create(appId, roleName, ownerId, modifiedByUserId);
            return result;
        }

        [HttpPost("{appId}/role-adduser")]
        public async Task<IActionResult> Role_AddUser(string appId, Guid roleId, Guid userId, Guid modifiedByUserId)
        {
            //todo: check permission

            await _authManager.Role_AddUser(appId, roleId, userId, modifiedByUserId);
            return Ok();
        }

        [HttpGet("{appId}/roles")]
        public async Task<Role[]> Role_List(string appId)
        {
            //todo: check permission

            var result = await _authManager.Role_List(appId);

            return result;
        }
        [HttpGet("{appId}/role-users")]
        public async Task<List<UserDto>> Role_Users(string appId, Guid roleId)
        {
            //todo: check permission

            var result = await _authManager.Role_Users(appId, roleId);

            return result;
        }

        [HttpGet("{appId}/user-roles")]
        public async Task<List<RoleDto>> User_Roles(string appId, Guid userId)
        {
            //todo: check permission

            var result = await _authManager.User_Roles(appId, userId);

            return result;
        }
    }
}
