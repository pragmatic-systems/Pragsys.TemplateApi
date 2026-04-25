using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Prometheus;
using Serilog;
using System;
using System.Linq;
using System.Security.Claims;
using Template.TestedApi.Api.HostedServices;
using Template.TestedApi.Api.Middleware;
using Template.TestedApi.Core.Validators;
using Template.TestedApi.Database;

namespace Template.TestedApi.Api;
public static class ConfigurationExtensions
{
    public static IApplicationBuilder UseHttpsRedirectionExcluding(this IApplicationBuilder builder, string excluding)
    {
        builder.UseWhen(
            context => !context.Request.Path.StartsWithSegments(excluding),
            builder => builder.UseHttpsRedirection());

        return builder;
    }

    public static IServiceCollection WithMediatr(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(InsertTodoValidator)));
        return services;
    }

    public static IServiceCollection WithPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DbContextOptions<ApplicationDbContext>>(s =>
        {
            // Required for EF to support PG timezones
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            var config = s.GetRequiredService<IConfiguration>();
            var conn = config.GetConnectionString("PostgresDb");

            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(conn)
                .EnableDetailedErrors()
                .Options;
        });

        services.AddScoped<ApplicationDbContext>(s =>
        {
            var options = s.GetRequiredService<DbContextOptions<ApplicationDbContext>>();
            return new ApplicationDbContext(options);
        });

        services.AddHostedService<PostgresInitService>();

        return services;
    }

    public static IServiceCollection WithOpenIdConnect(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OAuthConfig>(
            configuration.GetSection("OpenIdConnect"));

        services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(s =>
        {
            var options = s.GetRequiredService<IOptions<OAuthConfig>>();
            var config = options.Value.OpenIdConfigUrl;

            return new ConfigurationManager<OpenIdConnectConfiguration>(config, new OpenIdConnectConfigurationRetriever());
        });

        services.AddAuthentication().AddJwtBearer();

        return services;
    }

    public static IServiceCollection WithAuthorizationPolicy(this IServiceCollection services)
    {
        services.AddAuthorization(
            authorizationOptions =>
            {
                authorizationOptions.AddPolicy(Roles.TodoListRead, policy => policy.RequireClaim(ClaimTypes.Role, Roles.TodoListRead));
                authorizationOptions.AddPolicy(Roles.TodoListWrite, policy => policy.RequireClaim(ClaimTypes.Role, Roles.TodoListWrite));
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

    public static IServiceCollection AddAppHealthChecks(this IServiceCollection services, IConfiguration configuration, bool testMode)
    {
        var healthcheckBuilder = services
            .AddHealthChecks()
            .AddNpgSql(s =>
            {
                return configuration.GetConnectionString("PostgresDb");
            });

        // NOTE: Suppress healthcheck for OIDC if we are in test mode, as it's a fake endpoint that won't exist.
        if (!testMode)
        {
            healthcheckBuilder.AddUrlGroup(
                s =>
                {
                    var config = configuration
                        .GetRequiredSection("OpenIdConnect:OpenIdConfigUrl")
                        .Value;

                    return new Uri(config);
                },
                "OIDC Provider");
        }

        return services;
    }

    public static WebApplication ConfigureSwagger(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    public static WebApplication UseAuthMiddleware(this WebApplication app)
    {
        app.UseMiddleware<AuthMiddleware>();
        return app;
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
}
