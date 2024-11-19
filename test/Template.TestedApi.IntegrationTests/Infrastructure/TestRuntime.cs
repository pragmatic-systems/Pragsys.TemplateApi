
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration.Memory;
using Testcontainers.PostgreSql;

namespace Template.TestedApi.IntegrationTests.Infrastructure;

public class TestRuntime : IAsyncDisposable
{
    private PostgreSqlContainer PostgresContainer;
    //private WireMockContainer WireMockContainer;

    public WebApplicationFactory<Template.TestedApi.Api.Program> TargetApi { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await TargetApi.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        PostgresContainer = new PostgreSqlBuilder()
            .WithAutoRemove(true)
            .Build();

        await PostgresContainer.StartAsync();

        var postgresConnection = PostgresContainer.GetConnectionString();

        TargetApi = new WebApplicationFactory<Api.Program>()
            .WithWebHostBuilder(builder =>
            {
                // Override this so we don't inherit any default config.
                builder.UseEnvironment("IntegrationTest");

                // Configure overrides for application
                builder.ConfigureAppConfiguration((c, b) =>
                {
                    var config = new MemoryConfigurationSource();

                    // Override config settings for connection strings / service urls here.
                    config.InitialData = new Dictionary<string, string?>
                    {
                        { "ConnectionStrings:PostgresDb", postgresConnection }
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