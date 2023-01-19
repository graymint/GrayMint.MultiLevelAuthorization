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

    public async Task<App> InitApp(int appId, Guid rootSecureObjectId, SecureObjectType[] secureObjectTypes, Permission[] permissions, PermissionGroup[] permissionGroups, bool removeOtherPermissionGroups = true)
    {
        // Validate SecureObjectTypes for System value in list
        var result = secureObjectTypes.FirstOrDefault(x => x.SecureObjectTypeName == "System");
        if (result != null)
            throw new Exception("The SecureObjectTypeName could not allow System as an input parameter.");

        // Prepare system secure object
        var secureObjectDto = await _secureObjectService.BuildSystemEntity(appId, rootSecureObjectId);

        // Prepare SecureObjectTypes to add System to passed list
        var secureObjectType = new SecureObjectType
        {
            SecureObjectTypeId = secureObjectDto.SecureObjectTypeId,
            SecureObjectTypeName = "System"
        };
        secureObjectTypes = secureObjectTypes.Concat(new[] { secureObjectType }).ToArray();

        // update types
        await _secureObjectService.UpdateSecureObjectTypes(appId, secureObjectTypes);
        await _permissionService.Update(appId, permissions);
        await _permissionService.UpdatePermissionGroups(appId, permissionGroups, removeOtherPermissionGroups);

        // Table function
        await _authRepo.SaveChangesAsync();

        var appInfo = await Get(appId);
        var appData = new App
        {
            AppId = appId,
            AppName = appInfo.AppName,
            SystemSecureObjectId = secureObjectDto.SecureObjectId
        };
        return appData;
    }

    public async Task<App> Create(AppCreateRequest request)
    {
        // Create auth.App
        var app = new AppModel
        {
            AppName = request.AppName,
            AuthorizationCode = await _authRepo.NewAuthorizationCode()
        };

        await _authRepo.AddEntity(app);
        await _authRepo.SaveChangesAsync();

        return app.ToDto();
    }

    public async Task ResetAuthorizationCode(int appId)
    {
        // get max token id
        var maxAuthCode = await _authRepo.NewAuthorizationCode();

        // update AuthorizationCode
        var app = await _authRepo.GetApp(appId);
        app.AuthorizationCode = maxAuthCode;
        await _authRepo.SaveChangesAsync();
    }
}
