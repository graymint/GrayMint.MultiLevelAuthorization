using GrayMint.MultiLevelAuthorization.Dtos;
using GrayMint.MultiLevelAuthorization.Models;
using GrayMint.MultiLevelAuthorization.Repositories;

namespace GrayMint.MultiLevelAuthorization.Services;

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

    private async Task<AppModel> Get(int appId)
    {
        var app = await _authRepo.GetApp(appId);
        return app;
    }

    public async Task<App> InitApp(int appId, Guid rootSecureObjectId, SecureObjectType[] secureObjectTypes, Permission[] permissions, PermissionGroup[] permissionGroups, bool removeOtherPermissionGroups = true)
    {
        if (rootSecureObjectId == Guid.Empty)
            throw new InvalidOperationException("Can not set default guid for rootSecureObjectId");

        var appInfo = await Get(appId);

        // Validate SecureObjectTypes for System value in list
        var result = secureObjectTypes.FirstOrDefault(x => x.SecureObjectTypeName == "System");
        if (result != null)
            throw new Exception("The SecureObjectTypeName could not allow System as an input parameter");

        // Prepare system secure object
        var secureObjectDto = await _secureObjectService.BuildSystemEntity(appId, rootSecureObjectId);
        //SecureObjectDto secureObjectDto = new SecureObjectDto(secureObject.SecureObjectId, secureObject.SecureObjectTypeId, secureObject.ParentSecureObjectId);

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
        await _authRepo.ExecuteSqlRawAsync(_secureObjectService.SecureObject_HierarchySql());
        await _authRepo.SaveChangesAsync();

        var appData = new App
        {
            AppName = appInfo.AppName,
            SystemSecureObjectId = secureObjectDto.SecureObjectId
        };
        return appData;
    }

    public async Task<int> Create(AppCreateRequestHandler request)
    {
        // Create auth.App
        var app = new AppModel
        {
            AppName = request.AppName
        };

        await _authRepo.AddEntity(app);
        await _authRepo.SaveChangesAsync();

        return app.AppId;
    }
}
