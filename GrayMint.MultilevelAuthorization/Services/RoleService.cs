using MultiLevelAuthorization.DtoConverters;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Services;

public class RoleService
{
    private readonly AuthRepo _authRepo;
    private readonly SecureObjectService _secureObjectService;

    public RoleService(AuthRepo authRepo, SecureObjectService secureObjectService)
    {
        _authRepo = authRepo;
        _secureObjectService = secureObjectService;
    }

    public async Task<Role[]> GetRoles(int appId)
    {
        var roleModels = await _authRepo.GetRoles(appId);
        return roleModels.Select(x => x.ToDto()).ToArray();
    }

    public async Task<Role> Create(int appId, string roleName, string ownerSecureObjectTypeId, string ownerSecureObjectId, Guid modifiedByUserId)
    {
        var secureObject = await _secureObjectService.GetSecureObjectByExternalId(appId, ownerSecureObjectTypeId, ownerSecureObjectId);

        var roleModel = new RoleModel
        {
            AppId = appId,
            OwnerSecureObjectId = secureObject.SecureObjectId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
            RoleId = Guid.NewGuid(),
            RoleName = roleName
        };
        await _authRepo.AddEntity(roleModel);
        await _authRepo.SaveChangesAsync();

        var role = await _authRepo.GetRole(appId, roleModel.RoleId);
        return role.ToDto();
    }

    public async Task AddUserToRole(int appId, Guid roleId, Guid userId, Guid modifiedByUserId)
    {
        var roleUser = new RoleUserModel
        {
            AppId = appId,
            RoleId = roleId,
            UserId = userId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId
        };
        await _authRepo.AddEntity(roleUser);
        await _authRepo.SaveChangesAsync();
    }

    public async Task<User[]> GetRoleUsers(int appId, Guid roleId)
    {
        var roleUserModels = await _authRepo.GetRoleUsers(appId, roleId);

        var users = roleUserModels
            .Select(x => new User { UserId = x.UserId })
            .ToArray();
        return users;
    }
}