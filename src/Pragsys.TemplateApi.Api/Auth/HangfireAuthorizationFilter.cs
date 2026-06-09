using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Pragsys.TemplateApi.Api.Auth;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // TODO: Get the context and validate JWT and permission for Hangfire.
        // var httpContext = context.GetHttpContext();
        return true;
    }
}
