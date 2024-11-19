
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration.Memory;

namespace Template.TestedApi.IntegrationTests.Infrastructure;

public class TestRuntime : IAsyncDisposable
{
    //private PostgreSqlContainer PostgresContainer;
    //private WireMockContainer WireMockContainer;

    public WebApplicationFactory<Template.TestedApi.Api.Program> TargetApi { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await TargetApi.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        TargetApi = new WebApplicationFactory<Api.Program>()
            .WithWebHostBuilder(builder =>
            {
                // Override this so we don't inherit any default config.
                builder.UseEnvironment("IntegrationTest");

                // Configure overrides for application
                builder.ConfigureAppConfiguration((c, b) =>
                {
                    var config = new MemoryConfigurationSource();

                    // TODO: Override config settings for service urls here.
                    config.InitialData = new Dictionary<string, string?>
                    {
                        { "Key", "Value" }
                    };

                    b.Add(config);
                });

                builder.ConfigureTestServices(services =>
                {
                    // TODO: Inject test override services
                });

                builder.UseDefaultServiceProvider(o =>
                {
                    // Force validation of DependencyInjection.
                    o.ValidateOnBuild = true;
                });
            });


    }
}