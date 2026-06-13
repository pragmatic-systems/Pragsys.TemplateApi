using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Pragsys.TemplateApi.Instrumentation;
using Prometheus;
using Serilog;

namespace Pragsys.TemplateApi.Api;

#pragma warning disable S1118
public class Program
#pragma warning restore S1118
{
    public static void Main(string[] args)
    {
#pragma warning disable S2139
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            var testMode = builder.Environment.EnvironmentName == "IntegrationTest";

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.WithIngressConfig();
            builder.Services.WithSerilog(builder.Configuration, "Pragsys.TemplateApi API");
            builder.Services.WithPostgres(builder.Configuration);
            builder.Services.WithMediatr();
            builder.Services.WithOpenIdConnect(builder.Configuration);
            builder.Services.WithAuthorizationPolicy();
            builder.Services.WithHangfire(builder.Configuration);
            builder.Services.AddControllers();
            builder.Services.AddAppHealthChecks(builder.Configuration, testMode);

            var app = builder.Build();

            app.MapInstrumentationEndpoints();
            app.ConfigureSwagger();
            app.UseMetricServer(); // https://github.com/prometheus-net/prometheus-net
            app.UseRouting();
            app.UseHttpMetrics();
            app.UseAuthentication();
            app.ConfigureAuthentication();
            app.ConfigureHangfireSessionManagement();
            app.UseHangfireDashboard();
            app.MapControllers();

            Log.Logger.Information("Starting Application");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error Starting Application");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
#pragma warning restore S2139
    }
}
