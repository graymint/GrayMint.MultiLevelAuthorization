using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Test.Helper;

namespace MultiLevelAuthorization.Test.Tests;

[TestClass]
public class RoleTest : BaseControllerTest
{
    [TestMethod]
    public async Task Role_CRUD()
    {
        var roleName = Guid.NewGuid().ToString();
        var ownerId = Guid.NewGuid();
        var modifiedByUserId = Guid.NewGuid();

        //-----------------
        // check : Successfully create new role
        //-----------------

        // Create a new Role
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, roleName, ownerId, modifiedByUserId);

        // assert output dto
        Assert.AreEqual(ownerId, role.OwnerId);
        Assert.AreEqual(roleName, role.RoleName);
        Assert.AreEqual(modifiedByUserId, role.ModifiedByUserId);

        // Retrieve data based on created role
        var result = await TestInit1.RolesClient.GetRolesAsync(TestInit1.AppId);

        // Assert 
        Assert.IsNotNull(result.SingleOrDefault(x => x.RoleName == roleName
                                        && x.OwnerId == ownerId
                                        && x.ModifiedByUserId == modifiedByUserId
                                        && x.RoleId == role.RoleId
                                        ));
    }

    [TestMethod]
    public async Task Role_AddUser()
    {
        //Prepare conditions
        var roleName = Guid.NewGuid().ToString();
        var ownerId = Guid.NewGuid();
        var modifiedByUserId = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();

        // Create role1        
        var role1 = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, roleName, ownerId, modifiedByUserId);

        // Create role2
        roleName = Guid.NewGuid().ToString();
        var role2 = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, roleName, ownerId, modifiedByUserId);

        // Create role3
        roleName = Guid.NewGuid().ToString();
        var role3 = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, roleName, ownerId, modifiedByUserId);

        // Add user1 to role1
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role1.RoleId, userId1, modifiedByUserId);

        // Add user2 to role2
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role2.RoleId, userId2, modifiedByUserId);

        // Add user3 to role2
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role2.RoleId, userId3, modifiedByUserId);

        // add user1 to role3
        await TestInit1.RolesClient.AddUserToRoleAsync(TestInit1.AppId, role3.RoleId, userId1, modifiedByUserId);

        //-----------------------------
        // check : Successfully retrieve user2 and user3 for role2
        //-----------------------------
        var users = await TestInit1.RolesClient.GetRoleUsersAsync(TestInit1.AppId, role2.RoleId);
        Assert.AreEqual(2, users.Count);
        Assert.IsNotNull(users.SingleOrDefault(x => x.UserId == userId2));
        Assert.IsNotNull(users.SingleOrDefault(x => x.UserId == userId3));

        //-----------------------------
        // check : Successfully retrieve role1 and role3 for user1
        //-----------------------------
        var roles = await TestInit1.UsersClient.GetUserRolesAsync(TestInit1.AppId, userId1);
        Assert.AreEqual(2, roles.Count);
        Assert.IsNotNull(roles.SingleOrDefault(x => x.RoleId == role1.RoleId));
        Assert.IsNotNull(roles.SingleOrDefault(x => x.RoleId == role3.RoleId));
    }
}

