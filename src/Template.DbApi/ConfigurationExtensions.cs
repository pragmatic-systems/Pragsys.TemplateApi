using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using static Dapper.SqlMapper;
using Microsoft.Extensions.Options;
using Template.DbApi.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Template.DbApi;
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
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining(typeof(ConfigurationExtensions)));
        return services;
    }

    public static IServiceCollection WithPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var writeConn = configuration.GetSection("Postgres:WriteConnection").Value;
        var readConn = configuration.GetSection("Postgres:ReadConnection").Value;
        services.AddSingleton<IConnectionFactory>(new PostgresConnectionFactory(writeConn, readConn));
        
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
        services.AddLogging(lb => {
            lb.ClearProviders();
            lb.AddSerilog(Log.Logger);
        });

        return services;
    }
}
