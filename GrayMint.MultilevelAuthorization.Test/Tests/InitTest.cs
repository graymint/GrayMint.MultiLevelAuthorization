using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrayMint.Common.Client;
using GrayMint.Common.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Persistence;
using MultiLevelAuthorization.Services;
using MultiLevelAuthorization.Test.Api;
using MultiLevelAuthorization.Test.Helper;

namespace MultiLevelAuthorization.Test.Tests;

[TestClass]
public class InitTest : BaseControllerTest
{
    [TestMethod]
    public async Task Init_Successfully()
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

        //-----------
        // check: Validate successfully created SecureObjectTypes
        //-----------
        var actualTypes = await TestInit1.AuthDbContext.SecureObjectTypes
            .Where(x => x.AppId == TestInit1.AppId)
            .ToArrayAsync();

        Assert.AreEqual(4, actualTypes.Length, "Validate count of output");

        //Validate created record ( Systematic and manually by api)
        Assert.IsNotNull(actualTypes.Single(x => x.SecureObjectTypeExternalId == newSecureObjectType1.SecureObjectTypeId));
        Assert.IsNotNull(actualTypes.Single(x => x.SecureObjectTypeExternalId == SecureObjectService.SystemSecureObjectId));

        //-----------
        // check: Validate successful created PermissionGroup
        //-----------
        var actualPermissionGroups = await TestInit1.AuthDbContext.PermissionGroups
            .Where(x => x.AppId == TestInit1.AppId)
            .ToArrayAsync();
        Assert.IsNotNull(actualPermissionGroups.Single(x => x.PermissionGroupExternalId == newPermissionGroup1.PermissionGroupId && x.PermissionGroupName == newPermissionGroup1.PermissionGroupName));
    }

    [TestMethod]
    public async Task PermissionGroups_CRUD()
    {
        // Create first types
        var newSecureObjectType1 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create first permission
        var newPermission = new Permission() { PermissionId = Permissions.All.Max(x => x.PermissionId) + 1, PermissionName = Guid.NewGuid().ToString() };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create first permissionGroup
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

        // Get system PermissionGroup properties
        var actualPermissionGroups = await TestInit1.AuthDbContext.PermissionGroups
            .Where(x => x.AppId == TestInit1.AppId)
            .ToArrayAsync();
        var projectOwnerId = actualPermissionGroups.Single(x => x.PermissionGroupName == "ProjectOwner").PermissionGroupId;
        var projectViewerId = actualPermissionGroups.Single(x => x.PermissionGroupName == "ProjectViewer").PermissionGroupId;
        var userBasicId = actualPermissionGroups.Single(x => x.PermissionGroupName == "UserBasic").PermissionGroupId;

        // Prepare PermissionGroup2
        var newPermissionGroup2 = new PermissionGroup()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<Permission> { newPermission }
        };
        var permissionGroups2 = PermissionGroups.All.Concat(new[] { newPermissionGroup2 }).ToArray();

        //-----------------------------
        // check : Successfully creation of PermissionGroup1 and PermissionGroup2
        //-----------------------------

        // Call App_Init api for second time and add PermissionGroup2
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups2,
            Permissions = permissions,
            RemoveOtherPermissionGroups = false
        });

        // Validate count of PermissionGroups
        await using var scope = TestInit1.WebApp.Services.CreateAsyncScope();
        var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        actualPermissionGroups = await authDbContext.PermissionGroups.Where(x => x.AppId == TestInit1.AppId).ToArrayAsync();
        Assert.AreEqual(5, actualPermissionGroups.Length);

        // Validate to exists PermissionGroup1 and PermissionGroup2
        Assert.IsNotNull(actualPermissionGroups.Single(x => x.PermissionGroupExternalId == newPermissionGroup1.PermissionGroupId && x.PermissionGroupName == newPermissionGroup1.PermissionGroupName));
        Assert.IsNotNull(actualPermissionGroups.Single(x => x.PermissionGroupExternalId == newPermissionGroup2.PermissionGroupId && x.PermissionGroupName == newPermissionGroup2.PermissionGroupName));

        // Prepare PermissionGroup3
        var newPermissionGroupName = Guid.NewGuid().ToString();
        newPermissionGroup2.PermissionGroupName = newPermissionGroupName;

        var newPermissionGroup3 = new PermissionGroup()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<Permission> { newPermission }
        };
        var permissionGroups3 = PermissionGroups.All.Concat(new[] { newPermissionGroup2, newPermissionGroup3 }).ToArray();

        //-----------------------------
        // check : Successfully delete SecureObjectType1, update PermissionGroups2 and create the PermissionGroups3 after third call the App_Init
        //-----------------------------

        // App_Init for third
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups3,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Retrieve PermissionGroups info
        actualPermissionGroups = await TestInit1.AuthDbContext.PermissionGroups.Where(x => x.AppId == TestInit1.AppId).ToArrayAsync();

        // PermissionGroup1 must be deleted
        Assert.IsNull(actualPermissionGroups.FirstOrDefault(x => x.PermissionGroupExternalId == newPermissionGroup1.PermissionGroupId));

        // PermissionGroup2 must be updated
        Assert.IsNotNull(actualPermissionGroups.Single(x => x.PermissionGroupExternalId == newPermissionGroup2.PermissionGroupId && x.PermissionGroupName == newPermissionGroupName));

        // PermissionGroup3 must be created
        Assert.IsNotNull(actualPermissionGroups.Single(x => x.PermissionGroupExternalId == newPermissionGroup3.PermissionGroupId && x.PermissionGroupName == newPermissionGroup3.PermissionGroupName));

        // validate PermissionGroupId for system object
        Assert.IsNotNull(actualPermissionGroups.Single(x => x.PermissionGroupId == projectOwnerId && x.PermissionGroupName == "ProjectOwner"));
        Assert.IsNotNull(actualPermissionGroups.Single(x => x.PermissionGroupId == projectViewerId && x.PermissionGroupName == "ProjectViewer"));
        Assert.IsNotNull(actualPermissionGroups.Single(x => x.PermissionGroupId == userBasicId && x.PermissionGroupName == "UserBasic"));
        Assert.AreEqual(5, actualPermissionGroups.Length);
    }

    [TestMethod]
    public async Task SecureObjectType_CRUD()
    {
        // Create new SecureObjectType
        var newSecureObjectType1 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid().ToString() };
        var newSecureObjectType2 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1, newSecureObjectType2 }).ToArray();

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

        // Retrieve all systematic SecureObjectTypes
        var actualSecureObjectTypes = await TestInit1.AuthDbContext.SecureObjectTypes.Where(x => x.AppId == TestInit1.AppId).ToArrayAsync();
        var secureObjectTypesUser = actualSecureObjectTypes.Single(x => x.SecureObjectTypeExternalId == "User").SecureObjectTypeId;
        var secureObjectTypesSystem = actualSecureObjectTypes.Single(x => x.SecureObjectTypeExternalId == "System").SecureObjectTypeId;
        var secureObjectTypesProject = actualSecureObjectTypes.Single(x => x.SecureObjectTypeExternalId == "Project").SecureObjectTypeId;

        // Update name of newSecureObjectType2
        var secureObjectTypeName = Guid.NewGuid().ToString();
        newSecureObjectType2.SecureObjectTypeId = secureObjectTypeName;

        // Prepare SecureObjectType3
        var newSecureObjectType3 = new SecureObjectType() { SecureObjectTypeId = Guid.NewGuid().ToString() };
        var secureObjectTypes3 = SecureObjectTypes.All.Concat(new[] { newSecureObjectType2, newSecureObjectType3 }).ToArray();

        //-------------------------
        // check : Successfully delete SecureObjectType1, update SecureObjectType2 and create the SecureObjectType3 after second call the App_Init
        //-------------------------

        // Call App_Init for second time
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes3,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Retrieve information again
        await using var scope = TestInit1.WebApp.Services.CreateAsyncScope();
        var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        actualSecureObjectTypes = await authDbContext.SecureObjectTypes.Where(x => x.AppId == TestInit1.AppId).ToArrayAsync();

        // SecureObjectType1 must be deleted
        Assert.IsNull(actualSecureObjectTypes.FirstOrDefault(x => x.SecureObjectTypeExternalId == newSecureObjectType1.SecureObjectTypeId));

        // SecureObjectType2 must be updated
        Assert.IsNotNull(actualSecureObjectTypes.Single(x => x.SecureObjectTypeExternalId == newSecureObjectType2.SecureObjectTypeId &&
                                                             x.SecureObjectTypeExternalId == secureObjectTypeName));

        // SecureObjectType3 must be created
        Assert.IsNotNull(actualSecureObjectTypes.Single(x => x.SecureObjectTypeExternalId == newSecureObjectType3.SecureObjectTypeId &&
                                       x.SecureObjectTypeExternalId == newSecureObjectType3.SecureObjectTypeId));

        // Make sure that systematic SecureObjectType never be updated
        Assert.IsNotNull(actualSecureObjectTypes.Single(x => x.SecureObjectTypeId == secureObjectTypesUser && x.SecureObjectTypeExternalId == "User"));
        Assert.IsNotNull(actualSecureObjectTypes.Single(x => x.SecureObjectTypeId == secureObjectTypesSystem && x.SecureObjectTypeExternalId == "System"));
        Assert.IsNotNull(actualSecureObjectTypes.Single(x => x.SecureObjectTypeId == secureObjectTypesProject && x.SecureObjectTypeExternalId == "Project"));

        // Validate count of output
        Assert.AreEqual(5, actualSecureObjectTypes.Length);
    }

    [TestMethod]
    public async Task SecureObjectType_invalid_operation_exception_is_expected_when_name_is_System_in_list()
    {
        // Create new SecureObjectType
        var secureObjectTypeName = "System";
        var newSecureObjectType1 = new SecureObjectType() { SecureObjectTypeId = secureObjectTypeName };
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

        //-----------------------
        //check : Invalid operation exception is expected when "System" passed as SecureObjectTypeName
        //-----------------------
        try
        {
            await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
            {
                SecureObjectTypes = secureObjectTypes,
                PermissionGroups = permissionGroups,
                Permissions = permissions,
                RemoveOtherPermissionGroups = false
            });
            Assert.Fail("Invalid operation exception is expected when SecureObjectTypeName is repetitive in database");
        }
        catch (Exception ex)
        {
            if (!ex.Message.Contains("The SecureObjectTypeName could not allow System as an input parameter"))
                Assert.Fail();
        }
    }

    [TestMethod]
    public async Task SecureObjectType_invalid_operation_exception_is_expected_when_name_is_duplicate_in_db()
    {
        // Create new SecureObjectType
        var secureObjectTypeName = Guid.NewGuid().ToString();
        var newSecureObjectType1 = new SecureObjectType() {SecureObjectTypeId = secureObjectTypeName };
        var newSecureObjectType2 = new SecureObjectType() {SecureObjectTypeId = secureObjectTypeName };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1, newSecureObjectType2 }).ToArray();

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

        //-----------------------
        //check : Invalid operation exception is expected when passed a duplicate name as SecureObjectTypeName
        //-----------------------
        try
        {
            await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
            {
                SecureObjectTypes = secureObjectTypes,
                PermissionGroups = permissionGroups,
                Permissions = permissions,
                RemoveOtherPermissionGroups = false
            });
            Assert.Fail("Invalid operation exception is expected when SecureObjectTypeName is repetitive in db");
        }
        catch (ApiException ex)
        {
            Assert.IsTrue(ex.Is<AlreadyExistsException>());
        }

    }

    [TestMethod]
    public async Task RootSecureObject_and_RootSecureObjectType_must_be_unique()
    {
        // Create new types
        var newSecureObjectType1 = new SecureObjectType() {SecureObjectTypeId = Guid.NewGuid().ToString() };
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

        // Try to call Init with another RootSecureObjectId
        try
        {
            await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
            {
                SecureObjectTypes = secureObjectTypes,
                PermissionGroups = permissionGroups,
                Permissions = permissions,
                RemoveOtherPermissionGroups = true
            });
            Assert.Fail("invalid operation is expected when RootSecureObjectId is different");
        }
        catch (ApiException ex)
        {
            Assert.IsTrue(ex.Is<InvalidOperationException>());
            if (!ex.Message.Contains("Wrong RootSecureObjectId.")) Assert.Fail();
        }
    }
}