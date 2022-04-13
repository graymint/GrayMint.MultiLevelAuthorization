using MultiLevelAuthorization.Test.Apis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLevelAuthorization.Test.Helper;
public class tUtil_Init : BaseControllerTest
{
    public async Task<AppDto> Init(string appId)
    {
        var appDto = new AppDto();
        // Create new types
        var newSecureObjectType1 = new SecureObjectTypeDto() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        var newPermission = new PermissionDto() { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = Guid.NewGuid().ToString() };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroupDto()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<PermissionDto> { newPermission }
        };
        var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();

        // Call App_Init api
        var controller = new AppController(HttpClient);
        appDto = await controller.InitAsync(appId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        return appDto;
    }
}




