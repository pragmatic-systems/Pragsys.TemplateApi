using Hangfire;

namespace Pragsys.TemplateApi.Worker;

/// <summary>
/// Background service that ensures the Hangfire server is properly started
/// and provides a hook for registering recurring jobs.
/// </summary>
public class BackgroundJobActivatorService : BackgroundService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<BackgroundJobActivatorService> _logger;

    public BackgroundJobActivatorService(
        IBackgroundJobClient backgroundJobClient,
        IRecurringJobManager recurringJobManager,
        ILogger<BackgroundJobActivatorService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker service starting. Hangfire server is active.");

        // Register any recurring jobs here
        RegisterRecurringJobs();

        return Task.CompletedTask;
    }

    private void RegisterRecurringJobs()
    {
        // Example: Add recurring jobs as needed
        // _recurringJobManager
        //    .AddOrUpdate("cleanup-job",
        //    () => Console.WriteLine("Processing..."),
        //    Cron.Minutely);
        _logger.LogInformation("Recurring jobs registered.");
    }
}
