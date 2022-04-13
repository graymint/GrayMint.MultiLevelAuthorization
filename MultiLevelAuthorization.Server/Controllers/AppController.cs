using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MultiLevelAuthorization.DTOs;
using MultiLevelAuthorization.Repositories;
using MultiLevelAuthorization.Server.DTOs;
using System.Net;
using System.Net.Mime;

namespace MultiLevelAuthorization.Server.Controllers
{
    [ApiController]
    [Route("/api/apps")]

    public class AppController : Controller
    {
        private readonly AuthManager _authManager;
        private readonly IOptions<AppOptions> _appOptions;
        private readonly IMapper _mapper;

        public AppController(AuthManager authManager, IOptions<AppOptions> appOptions, IMapper mapper)
        {
            _authManager = authManager;
            _appOptions = appOptions;
            _mapper = mapper;
        }

        [HttpPost]
        [Produces(MediaTypeNames.Text.Plain)]
        [Authorize(AuthenticationSchemes = "Robot", Roles = "AppCreator")]
        public async Task<string> Create(AppCreateRequest request)
        {
            var createRequestHandler = _mapper.Map<AppCreateRequestHandler>(request);

            var result = await _authManager.App_Create(createRequestHandler);
            return result;
        }

        [HttpPost("{appId}/init")]
        public async Task<AppDto> Init(string appId, AppInitRequest request)
        {
            //todo: check permission
            var result = await _authManager.App_Init(appId, request.RootSecureObjectId, request.RootSeureObjectTypeId, request.SecureObjectTypes, request.Permissions, request.PermissionGroups, request.RemoveOtherPermissionGroups);
            return result;
        }

        [HttpGet("{appId}/authentication-token")]
        [Produces(MediaTypeNames.Text.Plain)]
        [Authorize(AuthenticationSchemes = "Robot", Roles = "AppCreator")]
        public async Task<string> GetAuthenticationToken(string appId)
        {
            //todo: check permission

            var jwt = JwtTool.CreateSymmetricJwt(_appOptions.Value.AuthenticationKey, AppOptions.AuthIssuer, AppOptions.AuthIssuer,
                appId.ToString(), null, new[] { "AppUser" });
            return jwt;
        }
    }
}
