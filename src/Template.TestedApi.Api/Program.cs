using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Serilog;

namespace Template.TestedApi.Api;

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
            builder.Services.WithSerilog(builder.Configuration, "Template.TestedApi API");
            builder.Services.WithPostgres(builder.Configuration);
            builder.Services.WithMediatr();
            builder.Services.WithOpenIdConnect(builder.Configuration);
            builder.Services.WithAuthorizationPolicy();
            builder.Services.AddControllers();
            builder.Services.AddAppHealthChecks(builder.Configuration, testMode);

            var app = builder.Build();

            app.MapInstrumentationEndpoints();
            app.ConfigureSwagger();
            app.UseMetricServer(); // https://github.com/prometheus-net/prometheus-net
            app.UseRouting();
            app.UseHttpMetrics();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            Log.Logger.Information("Starting Application");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error Starting Application");
            throw;
        }
#pragma warning restore S2139
    }
}
