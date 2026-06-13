using System.Security.Claims;
using Hangfire.Dashboard;
using Pragsys.TemplateApi.Instrumentation;

namespace Pragsys.TemplateApi.Api.Auth;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // UseAuthentication() already runs before the Hangfire dashboard middleware,
        // so HttpContext.User is populated from the HangfireCookie scheme.
        // No async call needed — just inspect the already-authenticated principal.
        var httpContext = context.GetHttpContext();

        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.HasClaim(ClaimTypes.Role, Roles.HangfireDashboard);
    }
}
