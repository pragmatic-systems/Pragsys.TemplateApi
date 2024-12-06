using BoDi;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using Template.TestedApi.IntegrationTests.Infrastructure.Jwt;

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

        var accessTokenParameters = new AccessTokenParameters();
        var encodedAccessToken = JwtBearerAccessTokenFactory.Create(accessTokenParameters);
        TestContext.TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", encodedAccessToken);

        objectContainer.RegisterInstanceAs(TestRuntime);
        objectContainer.RegisterInstanceAs(TestContext);
    }
}

