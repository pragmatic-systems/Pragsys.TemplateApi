using BoDi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template.DbApi.AcceptanceTests.StepDefinitions;
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
            Uri = settings["Apis:Template.DbApi"]
        };

        objectContainer.RegisterInstanceAs(context);
    }
}

