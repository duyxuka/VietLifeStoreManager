using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace VietlifeStore.Pages;

[Collection(VietlifeStoreTestConsts.CollectionDefinitionName)]
public class Index_Tests : VietlifeStoreWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
