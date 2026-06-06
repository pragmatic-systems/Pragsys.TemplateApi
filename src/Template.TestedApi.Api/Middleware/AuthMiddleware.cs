using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Serilog;

namespace Template.TestedApi.Api.Middleware;

// See here:
// https://medium.com/@buffetbenjamin/keycloak-essentials-openid-connect-c7fa87d3129d
// https://medium.com/@stefannovak96/authenticating-net-with-keycloak-ae7ce3675110

// For mocking:
// https://xebia.com/blog/mock-your-openid-connect-provider/
public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptions<OAuthConfig> _authConfig;
    private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private readonly ILogger _logger;

    public AuthMiddleware(
        RequestDelegate next,
        IConfigurationManager<OpenIdConnectConfiguration> configurationManager,
        IOptions<OAuthConfig> authConfig,
        ILogger logger)
    {
        _next = next;
        _authConfig = authConfig;
        _configurationManager = configurationManager;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            var audience = _authConfig.Value.Audience;
            var issuer = _authConfig.Value.Issuer;

            if (!SkipAuth(context))
            {
                var headers = context.Request.Headers;
                if (!headers.ContainsKey(HeaderNames.Authorization))
                {
                    throw new SecurityTokenValidationException("No Authorization Token supplied.");
                }

                var authHeader = headers[HeaderNames.Authorization].ToString();
                var bearerToken = authHeader.Replace(Constants.Bearer, string.Empty, StringComparison.InvariantCultureIgnoreCase).Trim();
                var config = await _configurationManager.GetConfigurationAsync(context.RequestAborted);

                var validationParams = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKeys = config.SigningKeys,

                    // NOTE: This is only nescessary to suppor AWS Cognito as the client_id is the aud.
                    // If you are not using Cognito, you can remove this.
                    AudienceValidator = TokenValidators.ValidateAudienceOrClientId,
                };

                context.User = new JwtSecurityTokenHandler()
                    .ValidateToken(bearerToken, validationParams, out var _);

                BuildAwsIamClaimMap(context);
            }

            await _next.Invoke(context);
        }
        catch (SecurityTokenException ex)
        {
            _logger.Warning(ex, "Auth Error");
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.CompleteAsync();
        }
    }

    private static void BuildAwsIamClaimMap(HttpContext context)
    {
        // All our Claims for service to service from AWS Cognito are going to come in as scopes.
        // How we implement the naming of the scopes is really up to the implementor, for now we are
        // hard coding these values.

        // If we integrate with IAM, we can use Roles.
        var claims = context.User.Claims
            .Where(c => c.Type == "scope")
            .SelectMany(c => c.Value.Split(" "))
            .Select(c => c.Replace("todolist-permissions/", string.Empty));

        var identity = new ClaimsIdentity();
        foreach (var claim in claims)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, claim));
        }

        context.User.AddIdentity(identity);
    }

    private static bool SkipAuth(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        path = path.ToLower();

        if (path.StartsWith("/favicon.ico"))
            return true;

        return path.StartsWith("/_system");
    }
}

public class OAuthConfig
{
    public string? Issuer { get; set; }

    public string? Audience { get; set; }

    public string? OpenIdConfigUrl { get; set; }
}
