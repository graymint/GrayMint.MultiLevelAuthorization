using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Services;

public class UserService
{
    private readonly AuthRepo3 _authRepo;

    public UserService(AuthRepo3 authRepo)
    {
        _authRepo = authRepo;
    }
    public async Task<Role[]> GetUserRoles(int appId, Guid userId)
    {
        var roleViews = await _authRepo.GetUserRoles(appId, userId);

        var roles = roleViews
            .Select(x => new Role { RoleId = x.RoleId, RoleName = x.RoleName })
            .ToArray();

        return roles;
    }
}