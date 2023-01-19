using System;
using System.Security;
using System.Threading.Tasks;
using GrayMint.Common.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            RootSecureObjectId = Guid.NewGuid(),
            SecureObjectTypes = SecureObjectTypes.All,
            PermissionGroups = PermissionGroups.All,
            Permissions = Permissions.All,
            RemoveOtherPermissionGroups = true
        });

        var secureObjectL1 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, Guid.NewGuid(), SecureObjectTypes.Project.SecureObjectTypeId);
        var secureObjectL2 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, Guid.NewGuid(), SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL1.SecureObjectId);
        var secureObjectL3 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, Guid.NewGuid(), SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL2.SecureObjectId);
        var secureObjectL4 = await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, Guid.NewGuid(), SecureObjectTypes.Project.SecureObjectTypeId, secureObjectL3.SecureObjectId);

        // add guest1 to Role1
        var guest1 = Guid.NewGuid();
        var role1 = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, Guid.NewGuid().ToString(), secureObjectL1.SecureObjectId);
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role1.RoleId, guest1);

        // add guest2 to Role2
        var guest2 = Guid.NewGuid();
        var role2 = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, Guid.NewGuid().ToString(), secureObjectL1.SecureObjectId);
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role2.RoleId, guest2);

        // check : by default user does not have access
        await Assert_Inheritance(secureObjectL1.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId);
        await Assert_Inheritance(secureObjectL2.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId);
        await Assert_Inheritance(secureObjectL3.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId);
        await Assert_Inheritance(secureObjectL4.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId);

        await Assert_Inheritance(secureObjectL1.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId);
        await Assert_Inheritance(secureObjectL2.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId);
        await Assert_Inheritance(secureObjectL3.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId);
        await Assert_Inheritance(secureObjectL4.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId);

        // check: inheritance: add role1 to Level3 and it shouldn't access to Level1
        await TestInit1.SecuresObjectClient.AddRolePermissionAsync(TestInit1.AppId, secureObjectL3.SecureObjectId, role1.RoleId, PermissionGroups.ProjectViewer.PermissionGroupId, Guid.NewGuid());
        await TestInit1.SecuresObjectClient.AddRolePermissionAsync(TestInit1.AppId, secureObjectL1.SecureObjectId, role2.RoleId, PermissionGroups.ProjectViewer.PermissionGroupId, Guid.NewGuid());

        // assert
        await Assert_Inheritance(secureObjectL1.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId);
        await Assert_Inheritance(secureObjectL2.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId);
        await Assert_Inheritance(secureObjectL3.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId, false);
        await Assert_Inheritance(secureObjectL4.SecureObjectId, guest1, Permissions.ProjectRead.PermissionId, false);

        await Assert_Inheritance(secureObjectL1.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId, false);
        await Assert_Inheritance(secureObjectL2.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId, false);
        await Assert_Inheritance(secureObjectL3.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId, false);
        await Assert_Inheritance(secureObjectL4.SecureObjectId, guest2, Permissions.ProjectRead.PermissionId, false);
    }

    private async Task Assert_Inheritance(Guid secureObjectId, Guid userId, int permissionId, bool throwSecurityException = true)
    {
        try
        {
            await TestInit1.SecuresObjectClient.VerifyUserPermissionAsync(TestInit1.AppId, secureObjectId, userId, permissionId);

            if (throwSecurityException)
                Assert.Fail("access denied exception is expected.");
        }
        catch (ApiException ex)
        {
            if (throwSecurityException)
                Assert.IsTrue(ex.Is<SecurityException>());
        }
    }
}
