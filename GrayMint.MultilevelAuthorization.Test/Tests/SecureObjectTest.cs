using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrayMint.Common.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Services;
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
        var newSecureObjectType1 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid().ToString() };
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

        // Call App_Init api
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
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
        var result = await TestInit1.AuthDbContext.SecureObjects
            .Include(x => x.SecureObjectType)
            .Where(x => x.AppId == TestInit1.AppId)
            .ToArrayAsync();

        // Assert consequence
        Assert.IsNotNull(result.First(x => x.ParentSecureObjectId == null));
    }
    [TestMethod]
    public async Task SecureObject_CRUD_without_ParentSecureObjectId()
    {
        // Create new types
        var newSecureObjectType1 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create new permission
        var newPermission = new Permission() { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = Guid.NewGuid().ToString() };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create new permissionGroup
        var newPermissionGroup1 = new PermissionGroup
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<Permission> { newPermission }
        };
        var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();

        // Call App_Init api
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Prepare parameters
        var secureObjectId = Guid.NewGuid().ToString();

        // check : Successfully create new SecureObject with ParentSecureObjectId
        await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, newSecureObjectType1.SecureObjectTypeId, secureObjectId, SecureObjectService.SystemSecureObjectId);

        // Call api to get info based on created SecureObject
        var result = await TestInit1.AuthDbContext.SecureObjects
            .Include(x => x.SecureObjectType)
            .Where(x => x.AppId == TestInit1.AppId)
            .ToArrayAsync();

        // Assert consequence
        Assert.IsNotNull(result.First(x => x.SecureObjectExternalId == secureObjectId
                                    && x.SecureObjectType!.SecureObjectTypeExternalId == newSecureObjectType1.SecureObjectTypeId
                                    ));
    }
    [TestMethod]
    public async Task SecureObject_CRUD_with_ParentSecureObjectId()
    {
        // Create new types
        var newSecureObjectType1 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid().ToString() };
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

        // Call App_Init api
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroupDto,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Prepare parameters
        var secureObjectId = Guid.NewGuid().ToString();

        // Call api to create SecureObject to build new parent expect System
        await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, newSecureObjectType1.SecureObjectTypeId, secureObjectId,
            SecureObjectService.SystemSecureObjectId);

        //----------------------
        // check : Successfully create new SecureObject with another SecureObject expect system as a parent
        //----------------------
        secureObjectId = Guid.NewGuid().ToString();

        await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, secureObjectId, newSecureObjectType1.SecureObjectTypeId, secureObjectId);

        // Call api to get info based on created SecureObject
        var result = await TestInit1.AuthDbContext.SecureObjects
            .Include(x => x.SecureObjectType)
            .Where(x => x.AppId == TestInit1.AppId)
            .ToArrayAsync();

        // Assert consequence
        Assert.IsNotNull(result.SingleOrDefault(x => x.SecureObjectExternalId == secureObjectId
                                    && x.SecureObjectType!.SecureObjectTypeExternalId == newSecureObjectType1.SecureObjectTypeId
                                    ));
    }

    [TestMethod]
    public async Task SecureObject_AddRolePermission()
    {
        var modifiedByUserId = Guid.NewGuid();

        // Create new types
        var newSecureObjectType1 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid().ToString() };
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

        // Call App_Init api
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Create a new Role
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, "test", SecureObjectService.SystemSecureObjectId, SecureObjectService.SystemSecureObjectTypeId);

        // Add user to created role
        var userId1 = Guid.NewGuid();
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role.RoleId, userId1, modifiedByUserId);

        await TestInit1.SecuresObjectClient.AddRolePermissionAsync(TestInit1.AppId, SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId,
            role.RoleId, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        //---------------------
        // check : permissionName must be exists in the list of permission for userId1
        //---------------------
        var result = await TestInit1.SecuresObjectClient.GetUserPermissionsAsync(TestInit1.AppId, SecureObjectService.SystemSecureObjectTypeId,
            SecureObjectService.SystemSecureObjectId, userId1);
        Assert.IsNotNull(result.SingleOrDefault(x => x.PermissionName == permissionName1));
    }

    [TestMethod]
    public async Task Fail_whit_wrong_SecureObjectType()
    {
        // initialize system
        var type1 = Guid.NewGuid().ToString();
        var type2 = Guid.NewGuid().ToString();
        var newSecureObjectType1 = new SecureObjectType { SecureObjectTypeId = type1 };
        var newSecureObjectType2 = new SecureObjectType { SecureObjectTypeId = type2 };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1, newSecureObjectType2 }).ToArray();
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            RemoveOtherPermissionGroups = true
        });

        // create a secure object with type1
        await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, type1, Guid.NewGuid().ToString());

        // create another secure object with type2
        var type2SecureObject = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, type2, Guid.NewGuid().ToString());

        // try to create new secure object with incompatible secure object type
        try
        {
            await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, type1, Guid.NewGuid().ToString(), type2SecureObject.SecureObjectId);
            Assert.Fail("Invalid operation is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(InvalidOperationException), ex.ExceptionTypeName);
            Assert.IsNotNull(ex.Message.Contains("Wrong secure object type of parent."));
        }
    }

    [TestMethod]
    public async Task SecureObject_AddUserPermission()
    {
        var modifiedByUserId = Guid.NewGuid();

        // Create new types
        var newSecureObjectType1 = new SecureObjectType { SecureObjectTypeId = Guid.NewGuid().ToString() };
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

        // Call App_Init api
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // create another secure object
        var newSecureObjectId = Guid.NewGuid().ToString();
        var secureObject = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId,
            newSecureObjectType1.SecureObjectTypeId, newSecureObjectId);

        // Create a new Role
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, "test", secureObject.SecureObjectId, secureObject.SecureObjectTypeId);

        // Add user to created role
        var userId1 = Guid.NewGuid();
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role.RoleId, userId1, modifiedByUserId);


        await TestInit1.SecuresObjectClient.AddUserPermissionAsync(TestInit1.AppId, secureObject.SecureObjectTypeId, secureObject.SecureObjectId,
            userId1, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        // assert permissionName
        var result =
            await TestInit1.SecuresObjectClient.GetUserPermissionsAsync(TestInit1.AppId, secureObject.SecureObjectTypeId, secureObject.SecureObjectId, userId1);
        Assert.IsNotNull(result.SingleOrDefault(x => x.PermissionName == permissionName1));
    }

    [TestMethod]
    public async Task SecureObject_AddPermission()
    {
        var modifiedByUserId = Guid.NewGuid();

        // Create new types
        var newSecureObjectType1 = new SecureObjectType { SecureObjectTypeId = Guid.NewGuid().ToString() };
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

        // Call App_Init api
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Create a new Role
        var roleName = Guid.NewGuid().ToString();
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, roleName, SecureObjectService.SystemSecureObjectId, SecureObjectService.SystemSecureObjectTypeId);

        // Add user to created role
        var userId1 = Guid.NewGuid();
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role.RoleId, userId1, modifiedByUserId);

        // Grant specific permission to user
        await TestInit1.SecuresObjectClient.AddUserPermissionAsync(TestInit1.AppId, SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId, userId1, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        // Grant specific permission to role
        await TestInit1.SecuresObjectClient.AddRolePermissionAsync(TestInit1.AppId, SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId, role.RoleId, newPermissionGroup1.PermissionGroupId, modifiedByUserId);

        // assert
        var result = await TestInit1.SecuresObjectClient.GetUserPermissionsAsync(TestInit1.AppId, SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId, userId1);
        Assert.IsNotNull(result.SingleOrDefault(x => x.PermissionName == permissionName1));
        Assert.AreEqual(1, result.Count);
    }
}
