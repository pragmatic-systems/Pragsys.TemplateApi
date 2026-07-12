using Hangfire;

namespace Pragmatic.TemplateApi.Worker;

public static class ConfigurationExtensions
{
    public static IServiceCollection WithHangfireServer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<BackgroundJobActivatorService>();

        services.AddHangfireServer(options =>
        {
            options.Queues = new[]
            {
                "default",
                "jobs",
            };

            // Number of concurrent jobs per server
            options.WorkerCount = 5;

            // Queue poll interval
            options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
        });

        return services;
    }
}
