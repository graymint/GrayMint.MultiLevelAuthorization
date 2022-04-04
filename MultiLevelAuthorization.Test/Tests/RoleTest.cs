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
        var controller = new AuthorizationController(HttpClient);

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
}

