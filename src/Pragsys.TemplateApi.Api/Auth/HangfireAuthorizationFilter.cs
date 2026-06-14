using System.Security.Claims;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Pragsys.TemplateApi.Instrumentation;

namespace Pragsys.TemplateApi.Api.Auth;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // NOTE: We can't resolve the Synchronous check here as Hangfire does not expose an async Authorize endpoint.
        var user = httpContext.User;

        return user?.Identity?.IsAuthenticated == true &&
            user?.HasClaim(ClaimTypes.Role, Roles.HangfireDashboard) == true;
    }
}
