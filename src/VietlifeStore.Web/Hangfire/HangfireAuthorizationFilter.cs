using Hangfire.Dashboard;
using System.Threading.Tasks;

namespace VietlifeStore.Web.Hangfire
{
    public class HangfireAuthorizationFilter : IDashboardAsyncAuthorizationFilter
    {
        public Task<bool> AuthorizeAsync(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Chỉ cho phép user đã đăng nhập và có role admin
            return Task.FromResult(
                httpContext.User.Identity?.IsAuthenticated == true &&
                (httpContext.User.IsInRole("admin") || httpContext.User.IsInRole("Admin"))
            );
        }
    }
}
