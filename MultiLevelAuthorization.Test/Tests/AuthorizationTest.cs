using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Test.Apis;

namespace MultiLevelAuthorization.Test.Tests;

[TestClass]
public class AuthorizationTest : BaseControllerTest
{
    [TestMethod]
    public async Task Successfully_Init_And_Validate_Parameters_In_Create()
    {
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

        // Call Init api
        var controller = new AppController(HttpClient);
        await controller.InitAsync(AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        //-----------
        // check: Validate successfuly created SecureObjectTypes
        //-----------
        var actualTypes = await controller.SecureObjectTypesAsync(AppId);

        // Validate count of output
        Assert.AreEqual(4, actualTypes.Count());

        //Validate created record ( Systematic and manually by api)
        actualTypes.Single(x => x.SecureObjectTypeId == newSecureObjectType1.SecureObjectTypeId && x.SecureObjectTypeName == newSecureObjectType1.SecureObjectTypeName);
        actualTypes.Single(x => x.SecureObjectTypeName == "System");
        actualTypes.Single(x => x.SecureObjectTypeName == "User");
        actualTypes.Single(x => x.SecureObjectTypeName == "Project");

        //-----------
        // check: Validate successfuly created PermissionGroup
        //-----------
        var actualPermissionGroups = await controller.PermissionGroupsAsync(AppId);
        actualPermissionGroups.Single(x => x.PermissionGroupId == newPermissionGroup1.PermissionGroupId && x.PermissionGroupName == newPermissionGroup1.PermissionGroupName);
        actualPermissionGroups.Single(x => x.PermissionGroupName == "ProjectOwner");
        actualPermissionGroups.Single(x => x.PermissionGroupName == "ProjectViewer");
        actualPermissionGroups.Single(x => x.PermissionGroupName == "UserBasic");

        //-----------
        // check: Validate successfuly created PermissionGroupPermission and Permission
        //-----------
        //var actualPermissions = actualPermissionGroups.Single(x => x.PermissionGroupId == newPermissionGroup1.PermissionGroupId).Permissions;
        //actualPermissions.Single(x => x.PermissionId == newPermission.PermissionCode && x.PermissionName == newPermission.PermissionName);
    }

    [TestMethod]
    public async Task Init_And_Validate_CRUD_For_PermissionGroups()
    {
        // Create first types
        var newSecureObjectType1 = new SecureObjectTypeDto() { SecureObjectTypeId = Guid.NewGuid(), SecureObjectTypeName = Guid.NewGuid().ToString() };
        var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

        // Create first permission
        var newPermission = new PermissionDto() { PermissionCode = Permissions.All.Max(x => x.PermissionCode) + 1, PermissionName = Guid.NewGuid().ToString() };
        var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

        // Create first permissionGroup
        var newPermissionGroup1 = new PermissionGroupDto()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<PermissionDto> { newPermission }
        };
        var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();

        // Call Init api
        var controller = new AppController(HttpClient);
        await controller.InitAsync(AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Get system PermissionGroup properties
        var actualPermissionGroups = await controller.PermissionGroupsAsync(AppId);
        Guid projectOwnerId = actualPermissionGroups.Single(x => x.PermissionGroupName == "ProjectOwner").PermissionGroupId;
        Guid projectViewerId = actualPermissionGroups.Single(x => x.PermissionGroupName == "ProjectViewer").PermissionGroupId;
        Guid userBasicId = actualPermissionGroups.Single(x => x.PermissionGroupName == "UserBasic").PermissionGroupId;

        // Prepare PermissionGroup2
        var newPermissionGroup2 = new PermissionGroupDto()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<PermissionDto> { newPermission }
        };
        var permissionGroups2 = PermissionGroups.All.Concat(new[] { newPermissionGroup2 }).ToArray();

        // Call Init api for second time and add PermissionGroup2
        await controller.InitAsync(AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups2,
            Permissions = permissions,
            RemoveOtherPermissionGroups = false
        });

        // Validate count of PermissionGroups
        actualPermissionGroups = await controller.PermissionGroupsAsync(AppId);
        Assert.AreEqual(5, actualPermissionGroups.Count());

        // Vaidate to exists PermissionGroup1 and PermissionGroup2
        actualPermissionGroups.Single(x => x.PermissionGroupId == newPermissionGroup1.PermissionGroupId && x.PermissionGroupName == newPermissionGroup1.PermissionGroupName);
        actualPermissionGroups.Single(x => x.PermissionGroupId == newPermissionGroup2.PermissionGroupId && x.PermissionGroupName == newPermissionGroup2.PermissionGroupName);

        // Prepare PermissionGroup3
        var newPermissionGroupName = Guid.NewGuid().ToString();
        newPermissionGroup2.PermissionGroupName = newPermissionGroupName;

        var newPermissionGroup3 = new PermissionGroupDto()
        {
            PermissionGroupId = Guid.NewGuid(),
            PermissionGroupName = Guid.NewGuid().ToString(),
            Permissions = new List<PermissionDto> { newPermission }
        };
        var permissionGroups3 = PermissionGroups.All.Concat(new[] { newPermissionGroup2, newPermissionGroup3 }).ToArray();

        // Init for third
        await controller.InitAsync(AppId, new AppInitRequest
        {
            SecureObjectTypes = secureObjectTypes,
            PermissionGroups = permissionGroups3,
            Permissions = permissions,
            RemoveOtherPermissionGroups = true
        });

        // Retrieve PermissionGroups info
        actualPermissionGroups = await controller.PermissionGroupsAsync(AppId);

        // PermissionGroup1 must be delete
        Assert.IsNotNull(actualPermissionGroups.FirstOrDefault(x => x.PermissionGroupId == newPermissionGroup1.PermissionGroupId));

        // PermissionGroup2 must be update
        actualPermissionGroups.Single(x => x.PermissionGroupId == newPermissionGroup2.PermissionGroupId && x.PermissionGroupName == newPermissionGroupName);

        // PermissionGroup2 must be create
        actualPermissionGroups.Single(x => x.PermissionGroupId == newPermissionGroup3.PermissionGroupId && x.PermissionGroupName == newPermissionGroup3.PermissionGroupName);

        // validate PermissionGroupId for system object
        actualPermissionGroups.Single(x => x.PermissionGroupId == projectOwnerId && x.PermissionGroupName == "ProjectOwner");
        actualPermissionGroups.Single(x => x.PermissionGroupId == projectViewerId && x.PermissionGroupName == "ProjectViewer");
        actualPermissionGroups.Single(x => x.PermissionGroupId == userBasicId && x.PermissionGroupName == "UserBasic");

        Assert.Equals(5, actualPermissionGroups.Count);
    }

    public async Task Init_And_Validate_CRUD_For_PermissionGroupPermissions()
    {

    }
    public async Task Init_And_Validate_CRUD_For_SecureObjectType()
    {

    }

    //-----------
    // check: System object is not deleted
    //-----------

    #region RemarkTests

    //[TestMethod]
    //public async Task Seeding()
    //{
    //    await using var authDbContext = new TestAuthContext();

    //    // Create new base types
    //    var newSecureObjectType1 = new SecureObjectType(Guid.NewGuid(), Guid.NewGuid().ToString());
    //    var secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType1 }).ToArray();

    //    var maxPermissionId = authDbContext.Permissions.Max(x => (int?)x.PermissionId) ?? 100;
    //    var newPermission = new Permission(maxPermissionId + 1, Guid.NewGuid().ToString());
    //    var permissions = Permissions.All.Concat(new[] { newPermission }).ToArray();

    //    var newPermissionGroup1 = new PermissionGroup(Guid.NewGuid(), Guid.NewGuid().ToString())
    //    {
    //        Permissions = new List<Permission> { newPermission }
    //    };
    //    var permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup1 }).ToArray();
    //    await authDbContext.Init(secureObjectTypes, permissions, permissionGroups);

    //    await using (TestAuthContext vhContext2 = new())
    //    {

    //        //-----------
    //        // check: new type is inserted
    //        //-----------
    //        Assert.AreEqual(newSecureObjectType1.SecureObjectTypeName,
    //            vhContext2.SecureObjectTypes.Single(x => x.SecureObjectTypeId == newSecureObjectType1.SecureObjectTypeId).SecureObjectTypeName);

    //        //-----------
    //        // check: new permission is inserted
    //        //-----------
    //        Assert.AreEqual(newPermission.PermissionName,
    //            vhContext2.Permissions.Single(x => x.PermissionId == newPermission.PermissionId).PermissionName);

    //        //-----------
    //        // check new permission group is inserted
    //        //-----------
    //        Assert.AreEqual(newPermissionGroup1.PermissionGroupName,
    //            vhContext2.PermissionGroups
    //                .Single(x => x.PermissionGroupId == newPermissionGroup1.PermissionGroupId).PermissionGroupName);

    //        //-----------
    //        // check: new permission group permissions in inserted
    //        //-----------
    //        Assert.IsTrue(vhContext2.PermissionGroups.Include(x => x.Permissions)
    //            .Single(x => x.PermissionGroupId == newPermissionGroup1.PermissionGroupId)
    //            .Permissions.Any(x => x.PermissionId == newPermission.PermissionId));

    //        //-----------
    //        // check: System object is not deleted
    //        //-----------
    //        Assert.IsTrue(vhContext2.SecureObjectTypes.Any(x => x.SecureObjectTypeId == AuthManager.SystemSecureObjectTypeId));
    //        Assert.IsTrue(vhContext2.PermissionGroups.Any(x =>
    //            x.PermissionGroupId == AuthManager.SystemPermissionGroupId));
    //    }

    //    //-----------
    //    // check: update SecureObjectTypeName
    //    //-----------
    //    newSecureObjectType1.SecureObjectTypeName = "new-name_" + Guid.NewGuid();
    //    await authDbContext.Init(secureObjectTypes, permissions, permissionGroups);
    //    await using (TestAuthContext vhContext2 = new())
    //        Assert.AreEqual(newSecureObjectType1.SecureObjectTypeName, vhContext2.SecureObjectTypes.Single(x => x.SecureObjectTypeId == newSecureObjectType1.SecureObjectTypeId).SecureObjectTypeName);

    //    //-----------
    //    // check: add/remove SecureObjectTypeName
    //    //-----------
    //    SecureObjectType newSecureObjectType2 = new(Guid.NewGuid(), Guid.NewGuid().ToString());
    //    secureObjectTypes = SecureObjectTypes.All.Concat(new[] { newSecureObjectType2 }).ToArray();
    //    await authDbContext.Init(secureObjectTypes, permissions, permissionGroups);
    //    await using (TestAuthContext vhContext2 = new())
    //    {
    //        Assert.IsTrue(vhContext2.SecureObjectTypes.Any(x => x.SecureObjectTypeId == newSecureObjectType2.SecureObjectTypeId));
    //        Assert.IsFalse(vhContext2.SecureObjectTypes.Any(x => x.SecureObjectTypeId == newSecureObjectType1.SecureObjectTypeId));
    //    }

    //    //-----------
    //    // check: add/remove new PermissionGroup
    //    //-----------
    //    PermissionGroup newPermissionGroup2 = new(Guid.NewGuid(), Guid.NewGuid().ToString())
    //    {
    //        Permissions = new List<Permission> { newPermission }
    //    };
    //    permissionGroups = PermissionGroups.All.Concat(new[] { newPermissionGroup2 }).ToArray();
    //    await authDbContext.Init(secureObjectTypes, permissions, permissionGroups);
    //    await using (TestAuthContext vhContext2 = new())
    //    {
    //        Assert.IsTrue(vhContext2.PermissionGroups.Any(x => x.PermissionGroupId == newPermissionGroup2.PermissionGroupId));
    //        Assert.IsFalse(vhContext2.PermissionGroups.Any(x => x.PermissionGroupId == newPermissionGroup1.PermissionGroupId));
    //    }
    //}

    //[TestMethod]
    //public async Task Rename_permission_group()
    //{
    //    await using var authDbContext = new TestAuthContext();

    //    var secureObject = await authDbContext.AuthManager.CreateSecureObject(Guid.NewGuid(), SecureObjectTypes.Project);
    //    await authDbContext.SaveChangesAsync();

    //    //-----------
    //    // check: assigned permission group should remain intact after renaming its name
    //    //-----------
    //    var guest1 = Guid.NewGuid();

    //    Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObject.SecureObjectId, guest1, Permissions.ProjectRead));
    //    await authDbContext.AuthManager.SecureObject_AddUserPermission(secureObject, guest1,
    //        PermissionGroups.ProjectViewer, AuthManager.SystemUserId);
    //    PermissionGroups.ProjectViewer.PermissionGroupName = Guid.NewGuid().ToString();
    //    await authDbContext.Init(SecureObjectTypes.All, Permissions.All, PermissionGroups.All);
    //    Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObject.SecureObjectId, guest1, Permissions.ProjectRead));

    //    //-----------
    //    // check: used SecureObjectType should not be deleted
    //    //-----------
    //    try
    //    {
    //        authDbContext.SecureObjectTypes.Remove(SecureObjectTypes.Project);
    //        await authDbContext.SaveChangesAsync();
    //        Assert.Fail("No cascade expected for SecureObjectType!");
    //    }
    //    catch { /* ignored */ }
    //}

    //[TestMethod]
    //public async Task InheritanceAccess()
    //{
    //    await using var authDbContext = new TestAuthContext();

    //    var secureObjectL1 = await authDbContext.AuthManager.CreateSecureObject(Guid.NewGuid(), SecureObjectTypes.Project);
    //    var secureObjectL2 = await authDbContext.AuthManager.CreateSecureObject(Guid.NewGuid(), SecureObjectTypes.Project, secureObjectL1);
    //    var secureObjectL3 = await authDbContext.AuthManager.CreateSecureObject(Guid.NewGuid(), SecureObjectTypes.Project, secureObjectL2);
    //    var secureObjectL4 = await authDbContext.AuthManager.CreateSecureObject(Guid.NewGuid(), SecureObjectTypes.Project, secureObjectL3);

    //    // add guest1 to Role1
    //    var guest1 = Guid.NewGuid();
    //    var role1 = await authDbContext.AuthManager.Role_Create(Guid.NewGuid().ToString(), AuthManager.SystemUserId);
    //    await authDbContext.AuthManager.Role_AddUser(role1.RoleId, guest1, AuthManager.SystemUserId);

    //    // add guest2 to Role2
    //    var guest2 = Guid.NewGuid();
    //    var role2 = await authDbContext.AuthManager.Role_Create(Guid.NewGuid().ToString(), AuthManager.SystemUserId);
    //    await authDbContext.AuthManager.Role_AddUser(role2.RoleId, guest2, AuthManager.SystemUserId);

    //    //-----------
    //    // check: inheritance: add role1 to L3 and it shouldn't access to L1
    //    //-----------
    //    await authDbContext.SaveChangesAsync();
    //    Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL1.SecureObjectId, guest1, Permissions.ProjectRead));
    //    Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL2.SecureObjectId, guest1, Permissions.ProjectRead));
    //    Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL3.SecureObjectId, guest1, Permissions.ProjectRead));
    //    Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL4.SecureObjectId, guest1, Permissions.ProjectRead));

    //    Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL1.SecureObjectId, guest2, Permissions.ProjectRead));
    //    Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL2.SecureObjectId, guest2, Permissions.ProjectRead));
    //    Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL3.SecureObjectId, guest2, Permissions.ProjectRead));
    //    Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL4.SecureObjectId, guest2, Permissions.ProjectRead));


    //    await authDbContext.AuthManager.SecureObject_AddRolePermission(secureObjectL3, role1, PermissionGroups.ProjectViewer, AuthManager.SystemUserId);
    //    await authDbContext.AuthManager.SecureObject_AddRolePermission(secureObjectL1, role2, PermissionGroups.ProjectViewer, AuthManager.SystemUserId);
    //    await authDbContext.SaveChangesAsync();
    //    Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL1.SecureObjectId, guest1, Permissions.ProjectRead));
    //    Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL2.SecureObjectId, guest1, Permissions.ProjectRead));
    //    Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL3.SecureObjectId, guest1, Permissions.ProjectRead));
    //    Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL4.SecureObjectId, guest1, Permissions.ProjectRead));

    //    Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL1.SecureObjectId, guest2, Permissions.ProjectRead));
    //    Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL2.SecureObjectId, guest2, Permissions.ProjectRead));
    //    Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL3.SecureObjectId, guest2, Permissions.ProjectRead));
    //    Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL4.SecureObjectId, guest2, Permissions.ProjectRead));
    //}

    #endregion

}