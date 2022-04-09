using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Test.Apis;
using MultiLevelAuthorization.Test.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiLevelAuthorization.Test.Tests;
[TestClass]

public class SecureObjectTest : BaseControllerTest
{
    [TestMethod]
    public async Task Init_create_null_parent_for_SecureObject()
    {
        var controller = new AuthorizationController(HttpClient);
        var appDto = new AppDto();
        // Create new types
        var newSecureObjectType1 = new SecureObjectTypeDto() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        var newPermission = new PermissionDto() { PermissionCode = Permissions.All.Max(x => x.PermissionCode) + 1, PermissionName = Guid.NewGuid().ToString() };
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
        appDto = await controller.InitAsync(AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        //----------------------
        // check : Successfully create new SecureObject without ParentSecureObjectId in App_Init call
        //----------------------

        // Call api to get info based on created SecureObject
        var result = await controller.SecureObjectsAsync(AppId);

        // Assert consequence
        Assert.IsNotNull(result.First(x => x.SecureObjectId == appDto.SystemSecureObjectId
                                    && x.ParentSecureObjectId == null
                                    ));

    }
    [TestMethod]
    public async Task SecureObject_CRUD_without_ParentSecureObjectId()
    {
        var controller = new AuthorizationController(HttpClient);
        var appDto = new AppDto();
        // Create new types
        var newSecureObjectType1 = new SecureObjectTypeDto() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        var newPermission = new PermissionDto() { PermissionCode = Permissions.All.Max(x => x.PermissionCode) + 1, PermissionName = Guid.NewGuid().ToString() };
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
        appDto = await controller.InitAsync(AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        //// Prepare parameters
        //Guid secureObjectId = Guid.NewGuid();

        ////----------------------
        //// check : Successfully create new SecureObject without ParentSecureObjectId
        ////----------------------

        //// Call api to create SecureObject
        //await controller.SecureObjectAsync(AppId, secureObjectId, newSecureObjectType1.SecureObjectTypeId, null);

        //// Call api to get info based on created SecureObject
        //var result = await controller.SecureObjectsAsync(AppId);

        //// Assert consequence
        //Assert.IsNotNull(result.First(x => x.SecureObjectId == secureObjectId
        //                            && x.SecureObjectTypeId == newSecureObjectType1.SecureObjectTypeId
        //                            && x.ParentSecureObjectId == appDto.SystemSecureObjectId
        //                            ));
    }
    [TestMethod]
    public async Task SecureObject_CRUD_with_ParentSecureObjectId()
    {
        var controller = new AuthorizationController(HttpClient);
        var appDto = new AppDto();
        // Create new types
        var newSecureObjectType1 = new SecureObjectTypeDto() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        var newPermission = new PermissionDto() { PermissionCode = Permissions.All.Max(x => x.PermissionCode) + 1, PermissionName = Guid.NewGuid().ToString() };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroupDto()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<PermissionDto> { newPermission }
        };
        List<PermissionGroupDto> permissionGroupDto = new List<PermissionGroupDto>();
        permissionGroupDto.Add(newPermissionGroup1);
        var permissionGroups = permissionGroupDto;// PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();

        // Call App_Init api
        appDto = await controller.InitAsync(AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Prepare parameters
        Guid secureObjectId = Guid.NewGuid();

        Guid parentSecureObjectId = appDto.SystemSecureObjectId;

        // Call api to create SecureObject to build new parent expect System
        await controller.SecureObjectAsync(AppId, secureObjectId, newSecureObjectType1.SecureObjectTypeId, parentSecureObjectId);

        //----------------------
        // check : Successfully create new SecureObject with another SecureObject expect system as a parent
        //----------------------
        parentSecureObjectId = secureObjectId;
        secureObjectId = Guid.NewGuid();

        await controller.SecureObjectAsync(AppId, secureObjectId, newSecureObjectType1.SecureObjectTypeId, parentSecureObjectId);

        // Call api to get info based on created SecureObject
        var result = await controller.SecureObjectsAsync(AppId);

        // Assert consequence
        Assert.IsNotNull(result.First(x => x.SecureObjectId == secureObjectId
                                    && x.SecureObjectTypeId == newSecureObjectType1.SecureObjectTypeId
                                    && x.ParentSecureObjectId == parentSecureObjectId
                                    ));
    }

    //////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    // todo :  var systemSecureObject = await _authDbContext.SecureObjects.SingleOrDefaultAsync(x => x.AppId == appId && x.ParentSecureObjectId == null);

}
