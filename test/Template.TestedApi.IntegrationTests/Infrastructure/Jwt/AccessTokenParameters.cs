using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Template.TestedApi.IntegrationTests.Infrastructure.OpenId;

namespace Template.TestedApi.IntegrationTests.Infrastructure.Jwt;

public record AccessTokenParameters
{
    public AccessTokenParameters(string audience, string issuer, X509Certificate2 certificate, params Claim[] claims)
    {
        Audience = audience;
        Issuer = issuer;
        SigningCertificate = certificate;
        Claims = new List<Claim>(claims);
    }

    public AccessTokenParameters(string audience, string issuer, X509Certificate2 certificate, IEnumerable<Claim> claims):
        this(audience, issuer, certificate, claims.ToArray())
    {
    }

    public X509Certificate2 SigningCertificate { get; set; }
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public List<Claim> Claims { get; set; }

    public void AddOrReplaceClaim(string claimType, string claimValue)
    {
        var claim = Claims?.FirstOrDefault(x => x.Type == claimType);
        if (claim != null)
            Claims?.Remove(claim);

        Claims ??= new List<Claim>();
        Claims.Add(new Claim(claimType, claimValue));
    }
}