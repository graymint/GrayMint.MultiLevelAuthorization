using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Test.Api;
using MultiLevelAuthorization.Test.Helper;

namespace MultiLevelAuthorization.Test.Tests;
[TestClass]

public class SecureObjectTest : BaseControllerTest
{
    [TestMethod]
    public async Task Init_create_null_parent_for_SecureObject()
    {
        // Create new types
        var newSecureObjectType1 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        var newPermission = new Permission() { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = Guid.NewGuid().ToString() };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroup()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<Permission> { newPermission }
        };
        var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();
        var rootSecureObjectId1 = Guid.NewGuid();

        // Call App_Init api
        var appDto = await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
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
        var result = await TestInit1.SecuresObjectClient.GetSecureObjectsAsync(TestInit1.AppId);

        // Assert consequence
        Assert.IsNotNull(result.First(x => x.SecureObjectId == appDto.SystemSecureObjectId
                                    && x.ParentSecureObjectId == null
                                    ));

    }
    [TestMethod]
    public async Task SecureObject_CRUD_without_ParentSecureObjectId()
    {
        // Create new types
        var newSecureObjectType1 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        var newPermission = new Permission() { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = Guid.NewGuid().ToString() };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroup()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<Permission> { newPermission }
        };
        var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();
        var rootSecureObjectId1 = Guid.NewGuid();

        // Call App_Init api
        var appDto = await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {

            RootSecureObjectId = rootSecureObjectId1,
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Prepare parameters
        var secureObjectId = Guid.NewGuid();

        //----------------------
        // check : Successfully create new SecureObject without ParentSecureObjectId
        //----------------------

        // Call api to create SecureObject
        await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, secureObjectId, newSecureObjectType1.SecureObjectTypeId);

        // Call api to get info based on created SecureObject
        var result = await TestInit1.SecuresObjectClient.GetSecureObjectsAsync(TestInit1.AppId);

        // Assert consequence
        Assert.IsNotNull(result.First(x => x.SecureObjectId == secureObjectId
                                    && x.SecureObjectTypeId == newSecureObjectType1.SecureObjectTypeId
                                    && x.ParentSecureObjectId == appDto.SystemSecureObjectId
                                    ));
    }
    [TestMethod]
    public async Task SecureObject_CRUD_with_ParentSecureObjectId()
    {
        // Create new types
        var newSecureObjectType1 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        var newPermission = new Permission { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = Guid.NewGuid().ToString() };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroup()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<Permission> { newPermission }
        };
        var permissionGroupDto = new List<PermissionGroup> { newPermissionGroup1 };

        var rootSecureObjectId1 = Guid.NewGuid();

        // Call App_Init api
        var appDto = await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {

            RootSecureObjectId = rootSecureObjectId1,
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroupDto,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Prepare parameters
        var secureObjectId = Guid.NewGuid();
        var parentSecureObjectId = appDto.SystemSecureObjectId;

        // Call api to create SecureObject to build new parent expect System
        await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, secureObjectId, newSecureObjectType1.SecureObjectTypeId, parentSecureObjectId);

        //----------------------
        // check : Successfully create new SecureObject with another SecureObject expect system as a parent
        //----------------------
        parentSecureObjectId = secureObjectId;
        secureObjectId = Guid.NewGuid();

        await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, secureObjectId, newSecureObjectType1.SecureObjectTypeId, parentSecureObjectId);

        // Call api to get info based on created SecureObject
        var result = await TestInit1.SecuresObjectClient.GetSecureObjectsAsync(TestInit1.AppId);


        // Assert consequence
        Assert.IsNotNull(result.SingleOrDefault(x => x.SecureObjectId == secureObjectId
                                    && x.SecureObjectTypeId == newSecureObjectType1.SecureObjectTypeId
                                    && x.ParentSecureObjectId == parentSecureObjectId
                                    ));
    }

    [TestMethod]
    public async Task SecureObject_AddRolePermission()
    {
        var modifiedByUserId = Guid.NewGuid();

        // Create new types
        var newSecureObjectType1 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        var permissionName1 = Guid.NewGuid().ToString();
        var newPermission = new Permission() { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = permissionName1 };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroup
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<Permission> { newPermission }
        };
        var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();
        var rootSecureObjectId1 = Guid.NewGuid();

        // Create a new Role
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, "test", Guid.NewGuid(), Guid.NewGuid());

        // Add user to created role
        var userId1 = Guid.NewGuid();
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role.RoleId, userId1, modifiedByUserId);

        // Call App_Init api
        var appDto = await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            RootSecureObjectId = rootSecureObjectId1,
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        await TestInit1.SecuresObjectClient.AddRolePermissionAsync(TestInit1.AppId, appDto.SystemSecureObjectId, role.RoleId, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        //---------------------
        // check : permissionName must be exists in the list of permission for userId1
        //---------------------
        var result = await TestInit1.SecuresObjectClient.GetUserPermissionsAsync(TestInit1.AppId, appDto.SystemSecureObjectId, userId1);
        Assert.IsNotNull(result.SingleOrDefault(x => x.PermissionName == permissionName1));
    }

    [TestMethod]
    public async Task SecureObject_AddUserPermission()
    {
        var modifiedByUserId = Guid.NewGuid();

        // Create new types
        var newSecureObjectType1 = new SecureObjectType { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        var permissionName1 = Guid.NewGuid().ToString();
        var newPermission = new Permission { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = permissionName1 };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroup
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<Permission> { newPermission }
        };
        var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();
        var rootSecureObjectId1 = Guid.NewGuid();

        // Create a new Role
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, "test", Guid.NewGuid(), Guid.NewGuid());

        // Add user to created role
        var userId1 = Guid.NewGuid();
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role.RoleId, userId1, modifiedByUserId);

        // Call App_Init api
        var app = await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            RootSecureObjectId = rootSecureObjectId1,
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        await TestInit1.SecuresObjectClient.AddUserPermissionAsync(TestInit1.AppId, app.SystemSecureObjectId, userId1, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        // assert permissionName
        var result =
            await TestInit1.SecuresObjectClient.GetUserPermissionsAsync(TestInit1.AppId, app.SystemSecureObjectId, userId1);
        Assert.IsNotNull(result.SingleOrDefault(x => x.PermissionName == permissionName1));
    }

    [TestMethod]
    public async Task SecureObject_AddPermission()
    {
        var modifiedByUserId = Guid.NewGuid();

        // Create new types
        var newSecureObjectType1 = new SecureObjectType { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        var permissionName1 = Guid.NewGuid().ToString();
        var newPermission = new Permission { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = permissionName1 };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroup
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<Permission> { newPermission }
        };

        var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();
        var rootSecureObjectId1 = Guid.NewGuid();

        // Create a new Role
        var roleName = Guid.NewGuid().ToString();
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, roleName, Guid.NewGuid(), Guid.NewGuid());

        // Add user to created role
        var userId1 = Guid.NewGuid();
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role.RoleId, userId1, modifiedByUserId);

        // Call App_Init api
        var appDto = await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            RootSecureObjectId = rootSecureObjectId1,
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Grant specific permission to user
        await TestInit1.SecuresObjectClient.AddUserPermissionAsync(TestInit1.AppId, appDto.SystemSecureObjectId, userId1, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        // Grant specific permission to role
        await TestInit1.SecuresObjectClient.AddRolePermissionAsync(TestInit1.AppId, appDto.SystemSecureObjectId, role.RoleId, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        // assert
        var result = await TestInit1.SecuresObjectClient.GetUserPermissionsAsync(TestInit1.AppId, appDto.SystemSecureObjectId, userId1);
        Assert.IsNotNull(result.SingleOrDefault(x => x.PermissionName == permissionName1));
        Assert.AreEqual(1, result.Count);
    }
}
