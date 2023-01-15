using GrayMint.Common.AspNetCore.SimpleRoleAuthorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MultiLevelAuthorization.Persistence;

namespace MultiLevelAuthorization.Server;

public class SimpleRoleProvider : ISimpleRoleProvider
{
    private readonly AuthDbContext _dbContext;
    private readonly AppSettings _appSettings;

    public SimpleRoleProvider(AuthDbContext dbContext, IOptions<AppSettings> appSettings)
    {
        _dbContext = dbContext;
        _appSettings = appSettings.Value;
    }

    public async Task<SimpleUser?> FindSimpleUserByEmail(string email)
    {
        // check to AppCreator
        if (email == AppSettings.AppCreatorEmail)
            return new SimpleUser
            {
                AuthorizationCode = _appSettings.AppCreatorAuthorizationCode,
                UserRoles = new[] { new SimpleUserRole(Roles.AppCreator, "*") }
            };

        // Check to AppUser
        var appIdString = email.Replace("@local", "");
        var app = await _dbContext.Apps.SingleAsync(x => x.AppId == int.Parse(appIdString));
        var simpleUser = new SimpleUser
        {
            AuthorizationCode = app.AuthorizationCode.ToString(),
            UserRoles = new[] { new SimpleUserRole(Roles.AppUser, app.AppId.ToString()) }
        };

        return simpleUser;
    }
}