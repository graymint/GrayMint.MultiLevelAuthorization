using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Test.Apis;
using MultiLevelAuthorization.Test.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiLevelAuthorization.Test.Tests;
[TestClass]


public class RoleTest : BaseControllerTest
{
    [TestMethod]
    public async Task Role_CRUD()
    {
        var controller = new RoleController(HttpClient);

        string roleName = Guid.NewGuid().ToString();
        Guid ownerId = Guid.NewGuid();
        Guid modifiedByUserId = Guid.NewGuid();

        //-----------------
        // check : Successfully create new role
        //-----------------

        // Create a new Role
        var role = await controller.RoleAsync(AppId, roleName, ownerId, modifiedByUserId);

        // Retrieve data based on created role
        var result = await controller.RolesAsync(AppId);

        // Assert 
        Assert.IsNotNull(result.Single(x => x.RoleName == roleName
                                        && x.OwnerId == ownerId
                                        && x.ModifiedByUserId == modifiedByUserId
                                        ));
    }

    [TestMethod]
    public async Task Role_AddUser()
    {
        var controller = new AuthorizationController(HttpClient);

        //Prepare conditions
        string roleName = Guid.NewGuid().ToString();
        Guid ownerId = Guid.NewGuid();
        Guid modifiedByUserId = Guid.NewGuid();
        Guid userId1 = Guid.NewGuid();
        Guid userId2 = Guid.NewGuid();
        Guid userId3 = Guid.NewGuid();

        // Create role1        
        var role1 = await controller.RoleAsync(AppId, roleName, ownerId, modifiedByUserId);

        // Create role2
        roleName = Guid.NewGuid().ToString();
        var role2 = await controller.RoleAsync(AppId, roleName, ownerId, modifiedByUserId);

        // Create role2
        roleName = Guid.NewGuid().ToString();
        var role3 = await controller.RoleAsync(AppId, roleName, ownerId, modifiedByUserId);

        // Add user1 to role1
        await controller.RoleAdduserAsync(AppId, role1.RoleId, userId1, modifiedByUserId);

        // Add user2 to role2
        await controller.RoleAdduserAsync(AppId, role2.RoleId, userId2, modifiedByUserId);

        // Add user3 to role2
        await controller.RoleAdduserAsync(AppId, role2.RoleId, userId3, modifiedByUserId);

        // add user1 to role3
        await controller.RoleAdduserAsync(AppId, role3.RoleId, userId1, modifiedByUserId);

        //-----------------------------
        // check : Successfully retrieve user2 and user3 for role2
        //-----------------------------
        var users = await controller.RoleUsersGetAsync(AppId, role2.RoleId);
        Assert.AreEqual(2, users.Count);
        Assert.IsNotNull(users.SingleOrDefault(x => x.UserId == userId2));
        Assert.IsNotNull(users.SingleOrDefault(x => x.UserId == userId3));

        //-----------------------------
        // check : Successfully retrieve role1 and role3 for user1
        //-----------------------------
        var roles = await controller.UserRolesAsync(AppId, userId1);
        Assert.AreEqual(2, roles.Count);
        Assert.IsNotNull(roles.SingleOrDefault(x => x.RoleId == role1.RoleId));
        Assert.IsNotNull(roles.SingleOrDefault(x => x.RoleId == role3.RoleId));

    }
}

