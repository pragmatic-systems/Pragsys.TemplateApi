using BoDi;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Security.Claims;
using Template.TestedApi.Api;
using Template.TestedApi.IntegrationTests.Infrastructure.Jwt;
using Template.TestedApi.IntegrationTests.Infrastructure.OpenId;

namespace Template.TestedApi.IntegrationTests.Infrastructure;

[Binding]
public class ScenarioSetup
{
    private readonly IObjectContainer objectContainer;

    public TestRuntime TestRuntime { get; private set; }
    public TestContext TestContext { get; private set; }

    public ScenarioSetup(IObjectContainer objectContainer)
    {
        this.objectContainer = objectContainer;
    }

    [BeforeScenario]
    public void ConfigureInjection()
    {
        TestRuntime = RuntimeSetup.TestRuntime;
        TestContext = new TestContext();
        TestContext.TestClient = TestRuntime.TargetApi.CreateClient();

        var audience = AppConstantsThatShouldBeConfig.Audience;
        var issuer = AppConstantsThatShouldBeConfig.Issuer;
        var signingCertificate = Consts.ValidSigningCertificate.ToX509Certificate2();
        var claims = new List<Claim>
        {
            new(AppClaimTypes.PermissionClaimType, Permissions.TodoListRead),
            new(AppClaimTypes.PermissionClaimType, Permissions.TodoListWrite)
        };

        var accessTokenParameters = new AccessTokenParameters(audience, issuer, signingCertificate, claims);
        var encodedAccessToken = JwtBearerAccessTokenFactory.Create(accessTokenParameters);
        TestContext.TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", encodedAccessToken);

        objectContainer.RegisterInstanceAs(TestRuntime);
        objectContainer.RegisterInstanceAs(TestContext);
    }
}

