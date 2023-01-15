using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiLevelAuthorization.Test.Apis;
using MultiLevelAuthorization.Test.Helper;

namespace MultiLevelAuthorization.Test.Tests;

[TestClass]
public class AppTest : BaseControllerTest
{
    [TestMethod]
    public async Task Success_clear_all()
    {
        // Create payment 
        var sampleApp = TestInit1.CreateSampleApp();

        // change token
        var appsClient = new AppsClient(TestInit1.HttpClientAppUser);
        await appsClient.ClearAllAsync(TestInit1.AppId);

        try
        {
            await TestInit1.PriceListsClient.GetAsync(TestInit1.AppId, sampleApp.RootPriceListDom.PriceList.PriceListId);
            Assert.Fail("Payment must be delete");
        }
        catch (ApiException ex) when (ex.StatusCode == (int)HttpStatusCode.NotFound)
        {
        }
    }
}