using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Threading;
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
                var config = await _configurationManager.GetConfigurationAsync(CancellationToken.None);

                var validationParams = new TokenValidationParameters
                {
                    ValidIssuer = config.Issuer,
                    ValidAudience = audience,
                    IssuerSigningKeys = config.SigningKeys,
                };

                context.User = new JwtSecurityTokenHandler()
                    .ValidateToken(bearerToken, validationParams, out var thing);
            }

            await _next.Invoke(context);
        }
        catch(SecurityTokenException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.CompleteAsync();
        }
    }

    private bool SkipAuth(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        path = path.ToLower();

        return path.StartsWith("/_system");
    }
}
