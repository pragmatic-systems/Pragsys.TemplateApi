using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Pragsys.TemplateApi.Api.Auth;

/// <summary>
/// Middleware that extracts a JWT token from the HangfireCookieJwt cookie and injects it
/// into the Authorization header before the JWT Bearer authentication handler processes it.
///
/// This allows the Hangfire dashboard to be accessed by passing a JWT as a cookie value
/// instead of an Authorization header, which is useful for browser-based access where
/// headers are harder to control.
///
/// Must be registered BEFORE UseAuthentication() in the pipeline.
/// </summary>
public class HangfireCookieJwtMiddleware
{
    public const string CookieName = "HangfireCookieJwt";

    private readonly RequestDelegate _next;

    public HangfireCookieJwtMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only process requests targeting the Hangfire dashboard path
        if (!context.Request.Path.StartsWithSegments("/hangfire"))
        {
            await _next(context);
            return;
        }

        // If there's already an Authorization header, don't override it
        if (context.Request.Headers.Authorization.Count > 0)
        {
            await _next(context);
            return;
        }

        // Try to extract JWT from a dedicated cookie
        var jwtToken = context.Request.Cookies[CookieName];

        if (!string.IsNullOrWhiteSpace(jwtToken))
        {
            // Inject the JWT into the Authorization header so the JWT Bearer handler picks it up
            context.Request.Headers.Authorization = $"Bearer {jwtToken}";
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register the HangfireCookieJwtMiddleware.
/// Call this BEFORE UseAuthentication() in the middleware pipeline.
/// </summary>
public static class HangfireCookieJwtMiddlewareExtensions
{
    public static IApplicationBuilder UseHangfireCookieJwt(this IApplicationBuilder app)
    {
        return app.UseMiddleware<HangfireCookieJwtMiddleware>();
    }
}
