using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Pragsys.TemplateApi.Instrumentation;

namespace Pragsys.TemplateApi.Api.Controllers;

[ApiController]
public class HangfireController : ControllerBase
{
    private readonly IClaimsTransformation _claimsTransformer;

    public HangfireController(
        IClaimsTransformation claimsTransformer)
    {
        _claimsTransformer = claimsTransformer;
    }

    // POST /hangfire-login - validate JWT, create cookie session
    [HttpPost("hangfire-login")]
    public async Task<IActionResult> Login()
    {
        try
        {
            var transformedPrincipal = await _claimsTransformer.TransformAsync(User);

            if (!transformedPrincipal.HasClaim(ClaimTypes.Role, Roles.HangfireDashboard))
                return Forbid();

            await HttpContext.SignInAsync("HangfireCookie", transformedPrincipal);
            return Redirect("/hangfire");
        }
        catch
        {
            return Unauthorized();
        }
    }

    // POST /hangfire-logout - logout
    [HttpPost("hangfire-logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("HangfireCookie");
        return Ok(new { message = "Logged out" });
    }
}
