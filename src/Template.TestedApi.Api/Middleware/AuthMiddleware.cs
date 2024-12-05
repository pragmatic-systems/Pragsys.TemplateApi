using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Template.TestedApi.Api.Middleware;

// See here:
// https://medium.com/@buffetbenjamin/keycloak-essentials-openid-connect-c7fa87d3129d
// https://medium.com/@stefannovak96/authenticating-net-with-keycloak-ae7ce3675110

// For mocking:
// https://xebia.com/blog/mock-your-openid-connect-provider/

// Note: Need to self sign a certificate and add it to trusted root to use local keycloak.
// https://www.supportyourtech.com/articles/how-to-add-certificate-to-trusted-root-windows-10-a-step-by-step-guide/

public class AuthMiddleware
{
    private RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        const string openIdConfigUrl = "https://localhost:8443/realms/test-realm/.well-known/openid-configuration";
        const string audience = "account";

        var headers = context.Request.Headers;
        if (!headers.ContainsKey(HeaderNames.Authorization))
        {
            throw new SecurityTokenValidationException("No Authorization Token supplied.");
        }

        var authHeader = headers[HeaderNames.Authorization].ToString();
        var bearerToken = authHeader.Replace("Bearer", string.Empty).Trim();

        var jwt = new JwtSecurityToken(bearerToken);
        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(openIdConfigUrl, new OpenIdConnectConfigurationRetriever());
        var config = await configManager.GetConfigurationAsync();

        var validationParams = new TokenValidationParameters
        {
            ValidIssuer = config.Issuer,
            ValidAudience = audience,
            IssuerSigningKeys = config.SigningKeys,
        };

        context.User = new JwtSecurityTokenHandler()
            .ValidateToken(bearerToken, validationParams, out var thing);

        await _next.Invoke(context);
    }
}
