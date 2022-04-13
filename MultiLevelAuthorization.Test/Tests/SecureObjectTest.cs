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
        var rootSecureObjectId1 = Guid.NewGuid();
        var rootSecureObjectTypeId1 = Guid.NewGuid();

        // Call App_Init api
        appDto = await controller.InitAsync(AppId, new AppInitRequest
        {
            RootSecureObjectId = rootSecureObjectId1,
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
        var rootSecureObjectId1 = Guid.NewGuid();
        var rootSecureObjectTypeId1 = Guid.NewGuid();

        // Call App_Init api
        appDto = await controller.InitAsync(AppId, new AppInitRequest
        {

            RootSecureObjectId = rootSecureObjectId1,
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Prepare parameters
        Guid secureObjectId = Guid.NewGuid();

        //----------------------
        // check : Successfully create new SecureObject without ParentSecureObjectId
        //----------------------

        // Call api to create SecureObject
        await controller.SecureObjectAsync(AppId, secureObjectId, newSecureObjectType1.SecureObjectTypeId, null);

        // Call api to get info based on created SecureObject
        var result = await controller.SecureObjectsAsync(AppId);

        // Assert consequence
        Assert.IsNotNull(result.First(x => x.SecureObjectId == secureObjectId
                                    && x.SecureObjectTypeId == newSecureObjectType1.SecureObjectTypeId
                                    && x.ParentSecureObjectId == appDto.SystemSecureObjectId
                                    ));
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
        var newPermission = new PermissionDto() { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = Guid.NewGuid().ToString() };
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

        var rootSecureObjectId1 = Guid.NewGuid();
        var rootSecureObjectTypeId1 = Guid.NewGuid();

        // Call App_Init api
        appDto = await controller.InitAsync(AppId, new AppInitRequest
        {

            RootSecureObjectId = rootSecureObjectId1,
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
        Assert.IsNotNull(result.SingleOrDefault(x => x.SecureObjectId == secureObjectId
                                    && x.SecureObjectTypeId == newSecureObjectType1.SecureObjectTypeId
                                    && x.ParentSecureObjectId == parentSecureObjectId
                                    ));
    }

    [TestMethod]
    public async Task SecureObject_AddRolePermission()
    {
        var controller = new AuthorizationController(HttpClient);
        var appDto = new AppDto();
        Guid modifiedByUserId = Guid.NewGuid();

        // Create new types
        var newSecureObjectType1 = new SecureObjectTypeDto() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        string permissinaName1 = Guid.NewGuid().ToString();
        var newPermission = new PermissionDto() { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = permissinaName1 };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroupDto()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<PermissionDto> { newPermission }
        };
        var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();
        var rootSecureObjectId1 = Guid.NewGuid();
        var rootSecureObjectTypeId1 = Guid.NewGuid();

        // Create a new Role
        var role = await controller.RoleAsync(AppId, "test", Guid.NewGuid(), Guid.NewGuid());

        // Add user to created role
        Guid userId1 = Guid.NewGuid();
        await controller.RoleAdduserAsync(AppId, role.RoleId, userId1, modifiedByUserId);

        // Call App_Init api
        appDto = await controller.InitAsync(AppId, new AppInitRequest
        {
            RootSecureObjectId = rootSecureObjectId1,
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        await controller.SecureobjectAddrolepermissionAsync
            (AppId, appDto.SystemSecureObjectId, role.RoleId, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        //---------------------
        // check : permissinaName must be exists in the list of permission for userId1
        //---------------------
        var result = await controller.Secureobject_userpermissionsAsync(AppId, appDto.SystemSecureObjectId, userId1);
        Assert.IsNotNull(result.SingleOrDefault(x => x.PermissionName == permissinaName1));
    }

    [TestMethod]
    public async Task SecureObject_AddUserPermission()
    {
        var controller = new AuthorizationController(HttpClient);
        var appDto = new AppDto();
        Guid modifiedByUserId = Guid.NewGuid();

        // Create new types
        var newSecureObjectType1 = new SecureObjectTypeDto() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        string permissinaName1 = Guid.NewGuid().ToString();
        var newPermission = new PermissionDto() { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = permissinaName1 };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroupDto()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<PermissionDto> { newPermission }
        };
        var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();
        var rootSecureObjectId1 = Guid.NewGuid();
        var rootSecureObjectTypeId1 = Guid.NewGuid();

        // Create a new Role
        var role = await controller.RoleAsync(AppId, "test", Guid.NewGuid(), Guid.NewGuid());

        // Add user to created role
        Guid userId1 = Guid.NewGuid();
        await controller.RoleAdduserAsync(AppId, role.RoleId, userId1, modifiedByUserId);

        // Call App_Init api
        appDto = await controller.InitAsync(AppId, new AppInitRequest
        {
            RootSecureObjectId = rootSecureObjectId1,
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        await controller.SecureobjectAdduserpermissionAsync
            (AppId, appDto.SystemSecureObjectId, userId1, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        //---------------------
        // check : permissinaName must be exists in the list of permission for userId1
        //---------------------
        var result = await controller.Secureobject_userpermissionsAsync(AppId, appDto.SystemSecureObjectId, userId1);
        Assert.IsNotNull(result.SingleOrDefault(x => x.PermissionName == permissinaName1));
    }

    [TestMethod]
    public async Task SecureObject_AddPermission()
    {
        var controller = new AuthorizationController(HttpClient);
        var appDto = new AppDto();
        Guid modifiedByUserId = Guid.NewGuid();

        // Create new types
        var newSecureObjectType1 = new SecureObjectTypeDto() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        string permissinaName1 = Guid.NewGuid().ToString();
        var newPermission = new PermissionDto() { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = permissinaName1 };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroupDto()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<PermissionDto> { newPermission }
        };
        // Create second permissionGroup
        var newPermissionGroup2 = new PermissionGroupDto()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<PermissionDto> { newPermission }
        };

        var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();
        var rootSecureObjectId1 = Guid.NewGuid();
        var rootSecureObjectTypeId1 = Guid.NewGuid();

        // Create a new Role
        var role = await controller.RoleAsync(AppId, "test", Guid.NewGuid(), Guid.NewGuid());

        // Add user to created role
        Guid userId1 = Guid.NewGuid();
        await controller.RoleAdduserAsync(AppId, role.RoleId, userId1, modifiedByUserId);

        // Call App_Init api
        appDto = await controller.InitAsync(AppId, new AppInitRequest
        {
            RootSecureObjectId = rootSecureObjectId1,
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Grant specific permission to user
        await controller.SecureobjectAdduserpermissionAsync
           (AppId, appDto.SystemSecureObjectId, userId1, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        // Grant specific permission to role
        await controller.SecureobjectAddrolepermissionAsync
          (AppId, appDto.SystemSecureObjectId, role.RoleId, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        //---------------------
        // check : permissinaName must be exists in the list of permission for userId1
        //---------------------
        var result = await controller.Secureobject_userpermissionsAsync(AppId, appDto.SystemSecureObjectId, userId1);
        Assert.IsNotNull(result.SingleOrDefault(x => x.PermissionName == permissinaName1));
    }

    //////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    // todo :  var systemSecureObject = await _authDbContext.SecureObjects.SingleOrDefaultAsync(x => x.AppId == appId && x.ParentSecureObjectId == null);

}
