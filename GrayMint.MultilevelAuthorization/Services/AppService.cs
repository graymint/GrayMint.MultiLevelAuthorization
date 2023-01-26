using MultiLevelAuthorization.DtoConverters;
using MultiLevelAuthorization.Dtos;
using MultiLevelAuthorization.Models;
using MultiLevelAuthorization.Repositories;

namespace MultiLevelAuthorization.Services;

public class AppService
{
    private readonly AuthRepo _authRepo;
    private readonly SecureObjectService _secureObjectService;
    private readonly PermissionService _permissionService;

    public AppService(
        AuthRepo authRepo,
        SecureObjectService secureObjectService,
        PermissionService permissionService
        )
    {
        _authRepo = authRepo;
        _secureObjectService = secureObjectService;
        _permissionService = permissionService;
    }

    public async Task<App> Get(int appId)
    {
        var app = await _authRepo.GetApp(appId);
        return app.ToDto();
    }

    public async Task<App> InitApp(int appId, SecureObjectType[] secureObjectTypes, Permission[] permissions, PermissionGroupsInitRequest[] permissionGroups, bool removeOtherPermissionGroups = true)
    {
        // Validate SecureObjectTypes for System value in list
        var result = secureObjectTypes.FirstOrDefault(x => x.SecureObjectTypeId == SecureObjectService.SystemSecureObjectTypeId);
        if (result != null)
            throw new Exception("The system SecureObjectTypeId is not accepted.");

        // Prepare SecureObjectTypes to add System to passed list
        var secureObjectType = new SecureObjectType
        {
            SecureObjectTypeId = SecureObjectService.SystemSecureObjectTypeId
        };
        secureObjectTypes = secureObjectTypes.Concat(new[] { secureObjectType }).ToArray();

        // update types
        await _authRepo.BeginTransaction();

        await _secureObjectService.UpdateSecureObjectTypes(appId, secureObjectTypes);
        await _permissionService.Update(appId, permissions);
        await _permissionService.UpdatePermissionGroups(appId, permissionGroups, removeOtherPermissionGroups);

        // Update system secure object
        await _secureObjectService.UpdateSystemSecureObject(appId);

        // commit transaction
        await _authRepo.CommitTransaction();

        var appData = new App
        {
            AppId = appId
        };
        return appData;
    }

    public async Task<App> Create()
    {
        // Create auth.App
        var app = new AppModel
        {
            AuthorizationCode = await _authRepo.GetNewAuthorizationCode()
        };

        await _authRepo.AddEntity(app);
        await _authRepo.SaveChangesAsync();

        // build system entities
        await _secureObjectService.BuildSystemSecureObject(app.AppId);

        return app.ToDto();
    }

    public async Task ResetAuthorizationCode(int appId)
    {
        // get max token id
        var maxAuthCode = await _authRepo.GetNewAuthorizationCode();

        // update AuthorizationCode
        var app = await _authRepo.GetApp(appId);
        app.AuthorizationCode = maxAuthCode;
        await _authRepo.SaveChangesAsync();
    }

    public async Task ClearAll(int appId)
    {
        await _authRepo.BeginTransaction();

        var sqlDeleteCommand = _authRepo.AppClearCommand(appId);

        await _authRepo.ExecuteSqlRaw(sqlDeleteCommand);
        await _authRepo.CommitTransaction();
    }
}
