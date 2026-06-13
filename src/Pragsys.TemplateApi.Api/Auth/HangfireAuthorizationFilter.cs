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
        var auth = httpContext.AuthenticateAsync("HangfireCookie").GetAwaiter().GetResult();

        return auth.Principal?.Identity.IsAuthenticated == true &&
            auth.Principal?.HasClaim(ClaimTypes.Role, Roles.HangfireDashboard) == true;
    }
}
