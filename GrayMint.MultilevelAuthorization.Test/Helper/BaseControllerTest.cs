using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MultiLevelAuthorization.Test.Helper;

public abstract class BaseControllerTest
{
    protected TestInit TestInit1 { get; private set; } = default!;

    [TestInitialize]
    public virtual async Task Init()
    {
        TestInit1 = await TestInit.Create();
    }
}