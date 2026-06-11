using DbUp;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Pragsys.CQRS;
using Pragsys.TemplateApi.Core.Validators;
using Pragsys.TemplateApi.Database;
using Pragsys.TemplateApi.Instrumentation.HostedServices;
using Prometheus;
using Serilog;

namespace Pragsys.TemplateApi.Instrumentation;

public static class ConfigurationExtensions
{
    public static IServiceCollection WithPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>((s, options) =>
        {
            var config = s.GetRequiredService<IConfiguration>();
            var conn = config.GetConnectionString("PostgresDb");

            options
                .UseNpgsql(conn)
                .UseSnakeCaseNamingConvention()
                .EnableDetailedErrors();
        });

        services.AddHostedService<PostgresInitService>();

        return services;
    }

    public static IServiceCollection WithMediatr(this IServiceCollection services)
    {
        services.AddCqrs(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(
                typeof(InsertTodoValidator).Assembly);
        });
        return services;
    }

    public static IServiceCollection WithSerilog(this IServiceCollection services, IConfiguration configuration, string appName)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.Console()
            .Enrich.WithProperty("App", appName)
            .CreateLogger();

        services.AddSingleton(Log.Logger);

        services.AddLogging(lb =>
        {
            lb.ClearProviders();
            lb.AddSerilog(Log.Logger);
        });

        return services;
    }

    public static WebApplication MapInstrumentationEndpoints(this WebApplication app)
    {
        app.UseHttpsRedirectionExcluding("/_system");

        app.MapMetrics("_system/metrics");
        app.MapHealthChecks("/_system/ping", new HealthCheckOptions { Predicate = _ => false });
        app.MapHealthChecks("/_system/health", new HealthCheckOptions
        {
            ResponseWriter = async (c, r) =>
            {
                var response = new
                {
                    Health = r.Status.ToString(),
                    Checks = r.Entries.Select(x =>
                        new
                        {
                            Health = x.Value.Status.ToString(),
                            Name = x.Key,
                        }),
                    Duration = r.TotalDuration,
                };
                await c.Response.WriteAsJsonAsync(response);
            },
        });

        return app;
    }

    public static IServiceCollection WithHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        // NOTE: There is no lazy load here for PostgreSQL - if the DB does not exist at this point it will fall over.
        services.AddHangfire(hfConfig =>
        {
            var connection = configuration.GetConnectionString("PostgresDb");

            ArgumentNullException.ThrowIfNull(connection, "PostgresDb Connection String");

            hfConfig
                .InitializeDatabase(connection);

            hfConfig
                .UsePostgreSqlStorage(connection);
        });

        return services;
    }

    public static IServiceCollection WithHangfireServer(this IServiceCollection services, IConfiguration configuration)
    {
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

    // NOTE: This step is required when launching locally and no existing DB is configured. There may be better ways to handle this for your local scenarios.
    public static IGlobalConfiguration InitializeDatabase(this IGlobalConfiguration configuration, string connectionString)
    {
        var retryPolicy = Policy
            .Handle<NpgsqlException>()
            .WaitAndRetry(
                10,
                i => TimeSpan.FromSeconds(2),
                (e, t) => Console.WriteLine("Retrying... Waiting for database"));

        retryPolicy.Execute(() =>
            EnsureDatabase.For.PostgresqlDatabase(connectionString));

        return configuration;
    }

    public static IApplicationBuilder UseHttpsRedirectionExcluding(this IApplicationBuilder builder, string excluding)
    {
        builder.UseWhen(
            context => !context.Request.Path.StartsWithSegments(excluding),
            builder => builder.UseHttpsRedirection());

        return builder;
    }
}
