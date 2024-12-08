using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Template.TestedApi.Core;

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
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(PostgresConnectionFactory)));
        return services;
    }

    public static IServiceCollection WithPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConnectionFactory>(s =>
        {
            var config = s.GetRequiredService<IConfiguration>();
            return new PostgresConnectionFactory(
                config.GetConnectionString("PostgresDb"),
                config.GetConnectionString("PostgresDb"));
        });

        return services;
    }

    public static IServiceCollection WithOpenIdConnect(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(s =>
        {
            var config = configuration
                .GetRequiredSection("OpenIdConnect:OpenIdConfigUrl")
                .Value;

            return new ConfigurationManager<OpenIdConnectConfiguration>(config, new OpenIdConnectConfigurationRetriever());
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
}
