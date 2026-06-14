using System.Security.Claims;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authentication;
using Pragsys.TemplateApi.Instrumentation;

namespace Pragsys.TemplateApi.Api.Auth;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var user = context
            .GetHttpContext()
            .User;

        return user?.Identity?.IsAuthenticated == true &&
            user?.HasClaim(ClaimTypes.Role, Roles.HangfireDashboard) == true;
    }
}
