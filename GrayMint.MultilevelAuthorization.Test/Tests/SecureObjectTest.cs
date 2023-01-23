using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GrayMint.Common.Client;
using GrayMint.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Services;
using MultiLevelAuthorization.Test.Api;
using MultiLevelAuthorization.Test.Helper;
using AppInitRequest = MultiLevelAuthorization.Test.Api.AppInitRequest;
using SecureObjectUpdateRequest = MultiLevelAuthorization.Test.Api.SecureObjectUpdateRequest;

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
        await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, newSecureObjectType1.SecureObjectTypeId, secureObjectId,
            SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

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

        // create first secure object
        var parentSecureObject = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, newSecureObjectType1.SecureObjectTypeId, secureObjectId,
            SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

        //----------------------
        // check : Successfully create new SecureObject with another SecureObject expect system as a parent
        //----------------------
        secureObjectId = Guid.NewGuid().ToString();

        await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, newSecureObjectType1.SecureObjectTypeId, secureObjectId,
            parentSecureObject.SecureObjectTypeId, parentSecureObject.SecureObjectId);

        // Call api to get info based on created SecureObject
        var result = await TestInit1.AuthDbContext.SecureObjects
            .Include(x => x.SecureObjectType)
            .Include(x => x.ParentSecureObject)
            .Where(x => x.AppId == TestInit1.AppId)
            .ToArrayAsync();

        // Assert consequence
        Assert.IsNotNull(result.SingleOrDefault(x =>
            x.SecureObjectExternalId == secureObjectId &&
            x.SecureObjectType!.SecureObjectTypeExternalId == newSecureObjectType1.SecureObjectTypeId &&
            x.ParentSecureObject!.SecureObjectExternalId == parentSecureObject.SecureObjectId));
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
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, "test", SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

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
        var secureObject = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, newSecureObjectType1.SecureObjectTypeId, newSecureObjectId,
            SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

        // Create a new Role
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, "test", secureObject.SecureObjectTypeId, secureObject.SecureObjectId);

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
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, roleName, SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

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

    [TestMethod]
    public async Task Fail_Move_when_app_of_the_secureObject_and_ParentSecureObject_are_not_same()
    {
        // Init app1
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = SecureObjectTypes.All
        });

        // create first secureObject on app1
        var secureObject1 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.User.SecureObjectTypeId,
            Guid.NewGuid().ToString(), SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

        // Init app2
        var testInit2 = await TestInit.Create();
        await testInit2.AppsClient.InitAsync(testInit2.AppId, new AppInitRequest
        {
            SecureObjectTypes = SecureObjectTypes.All
        });

        // create second secureObject on app2
        var secureObject2 = await testInit2.SecuresObjectClient.CreateAsync(testInit2.AppId, SecureObjectTypes.User.SecureObjectTypeId,
            Guid.NewGuid().ToString(), SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

        // try to move secureObject2 to secureObject1
        var request = new SecureObjectUpdateRequest
        {
            ParentSecureObjectTypeId = new PatchOfString { Value = secureObject2.SecureObjectTypeId },
            ParentSecureObjectId = new PatchOfString { Value = secureObject2.SecureObjectId }
        };

        try
        {
            await TestInit1.SecuresObjectClient.UpdateAsync(TestInit1.AppId, secureObject1.SecureObjectTypeId, secureObject1.SecureObjectId, request);
            Assert.Fail("Wrong app exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.IsTrue(ex.Is<NotExistsException>());
        }
    }

    [TestMethod]
    public async Task Fail_Move_when_secureObject_and_ParentSecureObject_are_same_but_not_belong_to_app()
    {
        // Init app1
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = SecureObjectTypes.All
        });

        // create first secureObject on app1
        var secureObject1 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.User.SecureObjectTypeId,
            Guid.NewGuid().ToString(), SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

        // create second secureObject on app1
        var secureObject2 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.User.SecureObjectTypeId,
            Guid.NewGuid().ToString(), SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

        // Init app2
        var testInit2 = await TestInit.Create();

        // try to move secureObject2 to secureObject1
        var request = new SecureObjectUpdateRequest
        {
            ParentSecureObjectTypeId = new PatchOfString { Value = secureObject2.SecureObjectTypeId },
            ParentSecureObjectId = new PatchOfString { Value = secureObject2.SecureObjectId }
        };

        try
        {
            await TestInit1.SecuresObjectClient.UpdateAsync(testInit2.AppId, secureObject1.SecureObjectTypeId, secureObject1.SecureObjectId, request);
            Assert.Fail("Forbidden exception is expected.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.StatusCode);
        }
    }

    [TestMethod]
    public async Task Fail_Move_when_secureObject_and_ParentSecureObject_are_same()
    {
        // Init app1
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = SecureObjectTypes.All
        });

        // create first secureObject on app1
        var secureObject = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.User.SecureObjectTypeId,
            Guid.NewGuid().ToString(), SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

        // try to move secureObject2 to secureObject1
        var request = new SecureObjectUpdateRequest
        {
            ParentSecureObjectTypeId = new PatchOfString { Value = secureObject.SecureObjectTypeId },
            ParentSecureObjectId = new PatchOfString { Value = secureObject.SecureObjectId }
        };

        try
        {
            await TestInit1.SecuresObjectClient.UpdateAsync(TestInit1.AppId, secureObject.SecureObjectTypeId, secureObject.SecureObjectId, request);
            Assert.Fail("SecureObject and ParentSecureObject can not be same.");
        }
        catch (ApiException ex)
        {
            Assert.IsTrue(ex.Is<InvalidOperationException>());
        }
    }

    [TestMethod]
    public async Task Success_Move()
    {
        // Init app1
        // Call App_Init api
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = SecureObjectTypes.All,
            PermissionGroups = PermissionGroups.All,
            Permissions = Permissions.All,
            RemoveOtherPermissionGroups = true
        });

        // create first secureObject
        var secureObject1 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.User.SecureObjectTypeId,
            Guid.NewGuid().ToString(), SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

        // create second secureObject
        var secureObject2 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.User.SecureObjectTypeId,
            Guid.NewGuid().ToString(), secureObject1.SecureObjectTypeId, secureObject1.SecureObjectId);

        // create third secureObject
        var secureObject3 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.User.SecureObjectTypeId,
            Guid.NewGuid().ToString(), secureObject2.SecureObjectTypeId, secureObject2.SecureObjectId);

        //create role
        var userId = Guid.NewGuid();
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, Guid.NewGuid().ToString(),
            secureObject3.SecureObjectTypeId, secureObject3.SecureObjectId);
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role.RoleId, userId);

        // add user to role for access to secureObject3
        await TestInit1.SecuresObjectClient.AddRolePermissionAsync(TestInit1.AppId, secureObject3.SecureObjectTypeId,
            secureObject3.SecureObjectId, role.RoleId, PermissionGroups.ProjectViewer.PermissionGroupId, Guid.NewGuid());

        // validate user must not have permission by default 
        Assert.IsFalse(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId,
            secureObject2.SecureObjectTypeId, secureObject2.SecureObjectId, userId, Permissions.ProjectRead.PermissionId));

        // move secureObject3
        var request = new SecureObjectUpdateRequest
        {
            ParentSecureObjectTypeId = new PatchOfString { Value = secureObject1.SecureObjectTypeId },
            ParentSecureObjectId = new PatchOfString { Value = secureObject1.SecureObjectId }
        };

        await TestInit1.SecuresObjectClient.UpdateAsync(TestInit1.AppId, secureObject3.SecureObjectTypeId, secureObject3.SecureObjectId, request);

        request = new SecureObjectUpdateRequest
        {
            ParentSecureObjectTypeId = new PatchOfString { Value = secureObject3.SecureObjectTypeId },
            ParentSecureObjectId = new PatchOfString { Value = secureObject3.SecureObjectId }
        };

        await TestInit1.SecuresObjectClient.UpdateAsync(TestInit1.AppId, secureObject2.SecureObjectTypeId, secureObject2.SecureObjectId, request);

        // validate user must have permission after move
        Assert.IsTrue(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId,
            secureObject2.SecureObjectTypeId, secureObject2.SecureObjectId, userId, Permissions.ProjectRead.PermissionId));
    }
}
