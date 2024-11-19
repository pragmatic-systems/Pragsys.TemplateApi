using BoDi;
using Microsoft.Extensions.Configuration;

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

        objectContainer.RegisterInstanceAs(TestRuntime);
        objectContainer.RegisterInstanceAs(TestContext);
    }
}

