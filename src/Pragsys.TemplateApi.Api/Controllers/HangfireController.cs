using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
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

    // POST /hangfire/login - pull raw bearer token from Authorization header,
    // write it directly into the HangfireCookieJwt cookie for browser persistence
    [HttpPost("login")]
    public IActionResult Login()
    {
        var authHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader))
            return Unauthorized();

        var bearerToken = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader["Bearer ".Length..]
            : authHeader;

        if (string.IsNullOrWhiteSpace(bearerToken))
            return Unauthorized();

        Response.Cookies.Append(
            HangfireCookieJwtMiddleware.CookieName,
            bearerToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = "/hangfire",
            });

        return Redirect("/hangfire/dashboard");
    }

    // GET /hangfire/login - convenience endpoint that sets the session cookie
    // when the JWT is provided via the HangfireCookieJwt cookie (browser access)
    [HttpGet("login")]
    public async Task<IActionResult> LoginGet()
    {
        return Login();
    }

    // POST /hangfire/logout - logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(HangfireCookieJwtMiddleware.CookieName);
        return Ok(new { message = "Logged out" });
    }
}
