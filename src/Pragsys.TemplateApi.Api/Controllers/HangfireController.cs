using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Pragsys.TemplateApi.Api.Auth;
using Pragsys.TemplateApi.Instrumentation;

namespace Pragsys.TemplateApi.Api.Controllers;

[ApiController]
[Route("hangfire")]
public class HangfireController : ControllerBase
{
    private readonly IClaimsTransformation _claimsTransformer;

    public HangfireController(
        IClaimsTransformation claimsTransformer)
    {
        _claimsTransformer = claimsTransformer;
    }

    // POST /hangfire/login - validate JWT (from Authorization header or HangfireCookieJwt cookie),
    // create HangfireCookie session cookie for browser persistence
    [HttpPost("login")]
    public async Task<IActionResult> Login()
    {
        try
        {
            var transformedPrincipal = await _claimsTransformer.TransformAsync(User);

            if (!transformedPrincipal.HasClaim(ClaimTypes.Role, Roles.HangfireDashboard))
                return Forbid();

            await HttpContext.SignInAsync(HangfireCookieJwtMiddleware.CookieName, transformedPrincipal);
            return Redirect("/hangfire/dashboard");
        }
        catch
        {
            return Unauthorized();
        }
    }

    // GET /hangfire/login - convenience endpoint that sets the session cookie
    // when the JWT is provided via the HangfireCookieJwt cookie (browser access)
    [HttpGet("login")]
    public async Task<IActionResult> LoginGet()
    {
        try
        {
            var transformedPrincipal = await _claimsTransformer.TransformAsync(User);

            if (!transformedPrincipal.HasClaim(ClaimTypes.Role, Roles.HangfireDashboard))
                return Forbid();

            await HttpContext.SignInAsync(HangfireCookieJwtMiddleware.CookieName, transformedPrincipal);
            return Redirect("/hangfire/dashboard");
        }
        catch
        {
            return Unauthorized();
        }
    }

    // POST /hangfire/logout - logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(HangfireCookieJwtMiddleware.CookieName);
        return Ok(new { message = "Logged out" });
    }
}
