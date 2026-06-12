using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pragsys.TemplateApi.Database;

namespace Pragsys.TemplateApi.Instrumentation.HostedServices;

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
        ArgumentException.ThrowIfNullOrEmpty(connection);

        // On Error: Check Db running :)
        Migrator.EnsureDb(connection);
        Migrator.Migrate(connection);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
