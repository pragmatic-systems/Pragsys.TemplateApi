using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Template.TestedApi.Api.HostedServices;

public class PostgresInitService : IHostedService
{
    private readonly IConfiguration _configuration;

    public PostgresInitService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = _configuration.GetConnectionString("PostgresDb");

        // On Error: Check Db running :)
        Migrator.Migrate(connection);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
