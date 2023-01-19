using MultiLevelAuthorization.DtoConverters;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Services;

public class RoleService
{
    private readonly AuthRepo _authRepo;

    public RoleService(AuthRepo authRepo)
    {
        _authRepo = authRepo;
    }

    public async Task<Role[]> GetRoles(int appId)
    {
        var roleModels = await _authRepo.GetRoles(appId);
        return roleModels.Select(x => x.ToDto()).ToArray();
    }

    public async Task<Role> Create(int appId, string roleName, Guid ownerSecureObjectId, Guid modifiedByUserId)
    {
        var role = new RoleModel
        {
            AppId = appId,
            OwnerSecureObjectId = ownerSecureObjectId,
            CreatedTime = DateTime.UtcNow,
            ModifiedByUserId = modifiedByUserId,
            RoleId = Guid.NewGuid(),
            RoleName = roleName
        };
        await _authRepo.AddEntity(role);
        await _authRepo.SaveChangesAsync();

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