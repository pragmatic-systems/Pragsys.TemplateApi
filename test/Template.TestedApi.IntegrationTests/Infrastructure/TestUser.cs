using System.Security.Claims;
using Template.TestedApi.IntegrationTests.Infrastructure.Jwt;
using Template.TestedApi.IntegrationTests.Infrastructure.OpenId;

namespace Template.TestedApi.IntegrationTests.Infrastructure;

public class TestUser
{
    public string UserName { get; private set; }

    public List<Claim> Claims { get; private set; } = new List<Claim>();

    public string UserJwt { get; private set; }

    public TestUser(string userName)
    {
        UserName = userName;
    }

    public void BuildJwt(PemCertificate certificate)
    {
        var audience = TestConstants.Audience;
        var issuer = TestConstants.Issuer;
        var signingCertificate = certificate.ToX509Certificate2();

        var accessTokenParameters = new AccessTokenParameters(audience, issuer, signingCertificate, Claims);
        UserJwt = JwtBearerAccessTokenFactory.Create(accessTokenParameters);
    }
}