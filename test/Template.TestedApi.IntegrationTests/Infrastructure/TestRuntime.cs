using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Serilog;
using Template.TestedApi.IntegrationTests.Infrastructure.OpenId;
using Testcontainers.PostgreSql;
using Template.TestedApi.Api;

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
                        { "ConnectionStrings:PostgresDb", postgresConnection },

                        { "OpenIdConnect:OpenIdConfigUrl", "https://blank/.well-known/openid-configuration" },
                        { "OpenIdConnect:Audience", AppConstantsThatShouldBeConfig.Audience }
                    };

                    b.Add(config);
                });

                builder.ConfigureTestServices(services =>
                {
                    var config = ConfigForMockedOpenIdConnectServer.Create(
                        OpenIdConnectDiscoveryDocumentConfigurationFactory.Create(AppConstantsThatShouldBeConfig.Issuer),
                        Consts.ValidSigningCertificate);

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
