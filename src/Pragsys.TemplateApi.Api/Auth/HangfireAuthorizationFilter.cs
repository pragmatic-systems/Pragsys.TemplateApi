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

        // Authenticate explicitly against the cookie scheme.
        // The cookie is set by the /hangfire-login endpoint after JWT validation.
        var result = httpContext.AuthenticateAsync("HangfireCookie").GetAwaiter().GetResult();

        return result.Succeeded && result.Principal.HasClaim(ClaimTypes.Role, Roles.HangfireDashboard);
    }
}
