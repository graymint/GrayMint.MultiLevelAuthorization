using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using GrayMint.Common.Client;
using GrayMint.Common.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Services;
using MultiLevelAuthorization.Test.Api;
using MultiLevelAuthorization.Test.Helper;

namespace MultiLevelAuthorization.Test.Tests;

[TestClass]
public class AppTest : BaseControllerTest
{
    [TestMethod]
    public async Task Success_clear_all()
    {
        // Initialize system
        await TestInit1.AppsClient.InitAsync(TestInit1.AppId, new AppInitRequest
        {
            SecureObjectTypes = SecureObjectTypes.All,
            PermissionGroups = PermissionGroups.All,
            Permissions = Permissions.All,
            RemoveOtherPermissionGroups = true
        });

        // Create SecureObject
        var secureObjectId = Guid.NewGuid().ToString();
        await TestInit1.SecuresObjectClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.User.SecureObjectTypeId, secureObjectId,
            SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId);

        // create role
        var role = await TestInit1.RolesClient.CreateAsync(TestInit1.AppId, SecureObjectTypes.User.SecureObjectTypeId,
            secureObjectId, Guid.NewGuid().ToString(), Guid.NewGuid());

        // add user to role
        await TestInit1.RolesClient.AddUserAsync(TestInit1.AppId, role.RoleId, Guid.NewGuid(), Guid.NewGuid());

        // add role permission
        await TestInit1.SecuresObjectClient.AddRolePermissionAsync(TestInit1.AppId, SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId,
            role.RoleId, PermissionGroups.ProjectOwner.PermissionGroupId, Guid.NewGuid());

        // add user permission
        var userId = Guid.NewGuid();
        await TestInit1.SecuresObjectClient.AddUserPermissionAsync(TestInit1.AppId, SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId,
            userId, PermissionGroups.ProjectOwner.PermissionGroupId, Guid.NewGuid());

        // change token
        await TestInit1.AppsClient.ClearAllAsync(TestInit1.AppId);

        try
        {
            await TestInit1.SecuresObjectClient.GetUserPermissionsAsync(TestInit1.AppId, SecureObjectService.SystemSecureObjectTypeId, SecureObjectService.SystemSecureObjectId, userId);
            Assert.Fail("Role must be delete");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual(nameof(NotExistsException), ex.ExceptionTypeName);
        }
    }

    [TestMethod]
    public async Task Fail_clear_all_in_production()
    {
        var appSettings = new Dictionary<string, string?>()
        {
            { "Auth:BotKey", Convert.ToBase64String(Guid.NewGuid().ToByteArray()) },
            { "ConnectionStrings:AuthDatabase",  "Server=(localdb)\\MSSQLLocalDB; initial catalog=EzPin_Authorization; Integrated Security=true;"}
        };

        // Create payment 
        var testInit = await TestInit.Create(appSettings, environment: "Production");
        try
        {
            await testInit.AppsClient.ClearAllAsync(testInit.AppId);
            Assert.Fail("This operation should not work in production.");
        }
        catch (ApiException ex)
        {
            Assert.AreEqual((int)HttpStatusCode.Forbidden, ex.StatusCode);
        }
    }
}
