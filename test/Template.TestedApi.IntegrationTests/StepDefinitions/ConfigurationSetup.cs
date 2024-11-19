using BoDi;
using Microsoft.Extensions.Configuration;
using Template.TestedApi.IntegrationTests.Infrastructure;

namespace Template.TestedApi.IntegrationTests.StepDefinitions;
[Binding]
public class ConfigurationSetup
{
    private readonly IObjectContainer objectContainer;

    public ConfigurationSetup(IObjectContainer objectContainer)
    {
        this.objectContainer = objectContainer;
    }

    [BeforeScenario]
    public void ConfigureInjection()
    {
        var env = Environment.GetEnvironmentVariable("env");

        var settings = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .Build();

        var context = new CoreTestContext
        {
            Uri = settings["Apis:Template.TestedApi"]
        };

        objectContainer.RegisterInstanceAs(context);
    }
}

