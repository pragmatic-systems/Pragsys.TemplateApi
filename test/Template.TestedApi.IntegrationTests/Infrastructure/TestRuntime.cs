using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Template.TestedApi.IntegrationTests.Infrastructure.Auth;
using Testcontainers.PostgreSql;

namespace Template.TestedApi.IntegrationTests.Infrastructure;

public class TestRuntime : IAsyncDisposable
{
    public PostgreSqlContainer PostgresContainer { get; private set; }
    //public WireMockContainer WireMockContainer { get; private set; }

    public WebApplicationFactory<Template.TestedApi.Api.Program> TargetApi { get; private set; }

    /// <summary>
    /// Certificate used for signing the JWT used by the API.
    /// </summary>
    public PemCertificate SigningCertificate { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await TargetApi.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        // Configure Postgres
        PostgresContainer = new PostgreSqlBuilder()
            .WithAutoRemove(true)
            .Build();

        await PostgresContainer.StartAsync();

        // Create SSL Certificate
        SigningCertificate = PemCertificate.Create();

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
                        { "ConnectionStrings:PostgresDb", PostgresContainer.GetConnectionString() },

                        { "OpenIdConnect:OpenIdConfigUrl", TestConstants.OpenIdConfigUrl },
                        { "OpenIdConnect:Audience", TestConstants.Audience },
                        { "OpenIdConnect:Issuer", TestConstants.Issuer }
                    };

                    b.Add(config);
                });

                builder.ConfigureTestServices(services =>
                {
                    var config = MockOpenIdConfigurationManagerBuilder.Create(
                        TestConstants.Issuer,
                        TestConstants.OpenIdConfigUrl,
                        SigningCertificate);

                    // Inject test override services
                    services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(config);
                });

                builder.UseDefaultServiceProvider(o =>
                {
                    // Force validation of DependencyInjection.
                    o.ValidateOnBuild = true;
                });
            });
    }
}