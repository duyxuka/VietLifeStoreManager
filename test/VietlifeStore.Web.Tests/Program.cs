using Microsoft.AspNetCore.Builder;
using VietlifeStore;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("VietlifeStore.Web.csproj"); 
await builder.RunAbpModuleAsync<VietlifeStoreWebTestModule>(applicationName: "VietlifeStore.Web");

public partial class Program
{
}
