using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Template.TestedApi.Api.Middleware;

// See here:
// https://medium.com/@buffetbenjamin/keycloak-essentials-openid-connect-c7fa87d3129d
// https://medium.com/@stefannovak96/authenticating-net-with-keycloak-ae7ce3675110

// For mocking:
// https://xebia.com/blog/mock-your-openid-connect-provider/

public class AuthMiddleware
{
    private RequestDelegate _next;
    private IConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private IConfiguration _configuration;

    public AuthMiddleware(RequestDelegate next, 
        IConfigurationManager<OpenIdConnectConfiguration> configurationManager, 
        IConfiguration configuration)
    {
        _next = next;
        _configurationManager = configurationManager;
        _configuration = configuration;
    }

    public async Task Invoke(HttpContext context)
    {

        try
        {
            var audience = _configuration
                .GetRequiredSection("OpenIdConnect:Audience")
                .Value;

            var issuer = _configuration
                .GetRequiredSection("OpenIdConnect:Issuer")
                .Value;

            if (!SkipAuth(context))
            {
                var headers = context.Request.Headers;
                if (!headers.ContainsKey(HeaderNames.Authorization))
                {
                    throw new SecurityTokenValidationException("No Authorization Token supplied.");
                }

                var authHeader = headers[HeaderNames.Authorization].ToString();
                var bearerToken = authHeader.Replace("Bearer", string.Empty).Trim();

                var jwt = new JwtSecurityToken(bearerToken);
                var config = await _configurationManager.GetConfigurationAsync(context.RequestAborted);

                var validationParams = new TokenValidationParameters
                {
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKeys = config.SigningKeys,

                    // NOTE: This is only nescessary to suppor AWS Cognito as the client_id is the aud.
                    // If you are not using Cognito, you can remove this.
                    AudienceValidator = TokenValidators.ValidateAudienceOrClientId
                };

                context.User = new JwtSecurityTokenHandler()
                    .ValidateToken(bearerToken, validationParams, out var thing);

                BuildAzureAdClaimMap(context);
                BuildAwsIamClaimMap(context);
            }

            await _next.Invoke(context);
        }
        catch(SecurityTokenException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.CompleteAsync();
        }
    }

    private static void BuildAzureAdClaimMap(HttpContext context)
    {
        // All claims come in as "ClaimTypes.Role" from AD, so we're going to find all roles and map them over to the AppClaimTypes.Permission role we have for the app.
        var claims = context.User.Claims
            .Where(c => c.Type == ClaimTypes.Role);
        
        var identity = new ClaimsIdentity();
        foreach (var claim in claims)
        {
            identity.AddClaim(new Claim(AppClaimTypes.Permission, claim.Value));
        }
        context.User.AddIdentity(identity);
    }

    private static void BuildAwsIamClaimMap(HttpContext context)
    {
        // All our "Claims" for service to service from AWS Cognito are going to come in as scopes.
        // How we implement the naming of the scopes is really up to the implementor, for now we are
        // hard coding these values.

        // If we integrate with IAM, we can use Roles, but this is for Service to Service and we are quite restricted.

        var claims = context.User.Claims
            .Where(c => c.Type == "scope")
            .SelectMany(c => c.Value.Split(" "))
            .Select(c => c.Replace("todolist-permissions/", ""));

        var identity = new ClaimsIdentity();
        foreach (var claim in claims)
        {
            identity.AddClaim(new Claim(AppClaimTypes.Permission, claim));
        }
        context.User.AddIdentity(identity);
    }

    private bool SkipAuth(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        path = path.ToLower();

        return path.StartsWith("/_system");
    }
}
