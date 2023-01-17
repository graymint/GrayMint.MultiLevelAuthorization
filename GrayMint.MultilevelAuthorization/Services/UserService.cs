using MultiLevelAuthorization.DtoConverters;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Services;

public class UserService
{
    private readonly AuthRepo _authRepo;

    public UserService(AuthRepo authRepo)
    {
        _authRepo = authRepo;
    }
    public async Task<Role[]> GetUserRoles(int appId, Guid userId)
    {
        var roleViews = await _authRepo.GetUserRoles(appId, userId);

        var roles = roleViews
            .Select(x => x.ToDto())
            .ToArray();

        return roles;
    }
}