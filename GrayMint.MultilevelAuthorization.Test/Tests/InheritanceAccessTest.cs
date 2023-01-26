using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Services;
using MultiLevelAuthorization.Test.Api;
using MultiLevelAuthorization.Test.Helper;

namespace MultiLevelAuthorization.Test.Tests;

[TestClass]
public class InheritanceAccessTest : BaseControllerTest
{
    [TestMethod]
    public async Task InheritanceAccess()
    {
        // Call App_Init api
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = SecureObjectTypes.All,
            PermissionGroups = PermissionGroups.All,
            Permissions = Permissions.All,
            RemoveOtherPermissionGroups = true
        });

        var secureObjectL1 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, Guid.NewGuid().ToString(), SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);
        var secureObjectL2 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, Guid.NewGuid().ToString(), secureObjectL1.SecureObjectTypeId, secureObjectL1.SecureObjectId);
        var secureObjectL3 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, Guid.NewGuid().ToString(), secureObjectL2.SecureObjectTypeId, secureObjectL2.SecureObjectId);
        var secureObjectL4 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, Guid.NewGuid().ToString(), secureObjectL3.SecureObjectTypeId, secureObjectL3.SecureObjectId);

        // add guest1 to Role1
        var guest1 = Guid.NewGuid();
        var role1 = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, secureObjectL1.SecureObjectTypeId, secureObjectL1.SecureObjectId, Guid.NewGuid().ToString());
        await TestInit1.RolesClient.AddUserAsync(TestInit1.AppId, role1.RoleId, guest1);


        // add guest2 to Role2
        var guest2 = Guid.NewGuid();
        var role2 = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId,  secureObjectL1.SecureObjectTypeId, secureObjectL1.SecureObjectId, Guid.NewGuid().ToString());
        await TestInit1.RolesClient.AddUserAsync(TestInit1.AppId, role2.RoleId, guest2);

        // check : by default user does not have access
        Assert.IsFalse(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL1.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId));
        Assert.IsFalse(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL2.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId));
        Assert.IsFalse(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL3.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId));
        Assert.IsFalse(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL4.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId));


        Assert.IsFalse(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL1.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId));
        Assert.IsFalse(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL2.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId));
        Assert.IsFalse(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL3.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId));
        Assert.IsFalse(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL4.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId));

        // check: inheritance: add role1 to Level3 and it shouldn't access to Level1
        await TestInit1.SecuresObjectClient.AddRolePermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL3.SecureObjectId, role1.RoleId, PermissionGroups.ProjectViewer.PermissionGroupId, Guid.NewGuid());
        await TestInit1.SecuresObjectClient.AddRolePermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL1.SecureObjectId, role2.RoleId, PermissionGroups.ProjectViewer.PermissionGroupId, Guid.NewGuid());

        // assert
        Assert.IsFalse(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL1.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId));
        Assert.IsFalse(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL2.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId));
        Assert.IsTrue(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL3.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId));
        Assert.IsTrue(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL4.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId));

        Assert.IsTrue(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL1.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId));
        Assert.IsTrue(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL2.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId));
        Assert.IsTrue(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL3.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId));
        Assert.IsTrue(await TestInit1.SecuresObjectClient.HasUserPermissionAsync(TestInit1.AppId, SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL4.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId));
    }
}
