using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Pragsys.CQRS;
using Prometheus;
using Serilog;
using Template.TestedApi.Api.Auth;
using Template.TestedApi.Api.HostedServices;
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

    public static IServiceCollection WithIngressConfig(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // NOTE: Evaluate alternative rate limiting policies appropriate for your application.
            // AddTokenBucketLimiter / AddSlidingWindowLimiter, etc.
            options.AddFixedWindowLimiter(policyName: "Basic", options =>
            {
                options.PermitLimit = 10;          // Allow 10 requests
                options.Window = TimeSpan.FromSeconds(10); // per 10-second window
                options.QueueLimit = 5;            // Queue up to 5 requests
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        services.Configure<KestrelServerOptions>(options =>
        {
            // 1mb request size
            options.Limits.MaxRequestBodySize = 1024 * 1024;
        });
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

    public static IServiceCollection WithOpenIdConnect(this IServiceCollection services, IConfiguration configuration)
    {
        var authSection = configuration.GetSection("OpenIdConnect");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // By setting the issuer as the Authority - the JWT bearer will load the OIDC Config from this location.
                options.Authority = authSection.GetValue<string>("Issuer");

                // NOTE: This uses any pre-loaded IConfigurationManager<OpenIdConnectConfiguration> which can be supplied by test runners.
                // If none is supplied, then it remains null and will be auto-initialized based off Authority.
                options.ConfigurationManager = services
                    .BuildServiceProvider()
                    .GetService<IConfigurationManager<OpenIdConnectConfiguration>>();

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAudience = authSection.GetValue<string>("Audience"),

                    // NOTE: Custom audience validator to support AWS Cognito where client_id is the aud.
                    // Remove if you are not using Cognito.
                    AudienceValidator = TokenValidators.ValidateAudienceOrClientId,
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    },
                };
            });

        // Transform AWS Cognito scope claims into role claims for policy-based authorization.
        services.AddSingleton<IClaimsTransformation, CognitoScopeToRoleClaimsTransformer>();

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
