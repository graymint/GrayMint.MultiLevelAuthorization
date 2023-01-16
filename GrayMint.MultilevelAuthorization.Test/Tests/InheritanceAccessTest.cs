using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Test.Helper;

namespace MultiLevelAuthorization.Test.Tests;

[TestClass]
public class InheritanceAccessTest : BaseControllerTest
{
    [TestMethod]
    public Task InheritanceAccess()
    {
        //await using var authDbContext = new TestAuthContext();

        //var secureObjectL1 = await authDbContext.AuthManager.CreateSecureObject(Guid.NewGuid(), SecureObjectTypes.Project);
        //var secureObjectL2 = await authDbContext.AuthManager.CreateSecureObject(Guid.NewGuid(), SecureObjectTypes.Project, secureObjectL1);
        //var secureObjectL3 = await authDbContext.AuthManager.CreateSecureObject(Guid.NewGuid(), SecureObjectTypes.Project, secureObjectL2);
        //var secureObjectL4 = await authDbContext.AuthManager.CreateSecureObject(Guid.NewGuid(), SecureObjectTypes.Project, secureObjectL3);

        //// add guest1 to Role1
        //var guest1 = Guid.NewGuid();
        //var role1 = await authDbContext.AuthManager.Role_Create(Guid.NewGuid().ToString(), AuthManager.SystemUserId);
        //await authDbContext.AuthManager.Role_AddUser(role1.RoleId, guest1, AuthManager.SystemUserId);

        //// add guest2 to Role2
        //var guest2 = Guid.NewGuid();
        //var role2 = await authDbContext.AuthManager.Role_Create(Guid.NewGuid().ToString(), AuthManager.SystemUserId);
        //await authDbContext.AuthManager.Role_AddUser(role2.RoleId, guest2, AuthManager.SystemUserId);

        ////-----------
        //// check: inheritance: add role1 to L3 and it shouldn't access to L1
        ////-----------
        //await authDbContext.SaveChangesAsync();
        //Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL1.SecureObjectId, guest1, Permissions.ProjectRead));
        //Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL2.SecureObjectId, guest1, Permissions.ProjectRead));
        //Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL3.SecureObjectId, guest1, Permissions.ProjectRead));
        //Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL4.SecureObjectId, guest1, Permissions.ProjectRead));

        //Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL1.SecureObjectId, guest2, Permissions.ProjectRead));
        //Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL2.SecureObjectId, guest2, Permissions.ProjectRead));
        //Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL3.SecureObjectId, guest2, Permissions.ProjectRead));
        //Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL4.SecureObjectId, guest2, Permissions.ProjectRead));


        //await authDbContext.AuthManager.SecureObject_AddRolePermission(secureObjectL3, role1, PermissionGroups.ProjectViewer, AuthManager.SystemUserId);
        //await authDbContext.AuthManager.SecureObject_AddRolePermission(secureObjectL1, role2, PermissionGroups.ProjectViewer, AuthManager.SystemUserId);
        //await authDbContext.SaveChangesAsync();
        //Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL1.SecureObjectId, guest1, Permissions.ProjectRead));
        //Assert.IsFalse(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL2.SecureObjectId, guest1, Permissions.ProjectRead));
        //Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL3.SecureObjectId, guest1, Permissions.ProjectRead));
        //Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL4.SecureObjectId, guest1, Permissions.ProjectRead));

        //Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL1.SecureObjectId, guest2, Permissions.ProjectRead));
        //Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL2.SecureObjectId, guest2, Permissions.ProjectRead));
        //Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL3.SecureObjectId, guest2, Permissions.ProjectRead));
        //Assert.IsTrue(await authDbContext.AuthManager.SecureObject_HasUserPermission(secureObjectL4.SecureObjectId, guest2, Permissions.ProjectRead));

        throw new NotImplementedException();
    }
}
