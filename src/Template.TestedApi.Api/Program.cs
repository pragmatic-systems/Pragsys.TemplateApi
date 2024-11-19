using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Serilog;
using Template.TestedApi.Core;

namespace Template.TestedApi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.WithSerilog(builder.Configuration, "Template.TestedApi API");
        builder.Services.WithPostgres(builder.Configuration);
        builder.Services.WithMediatr();
        builder.Services.AddHealthChecks();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirectionExcluding("/_system");

        // https://github.com/prometheus-net/prometheus-net

        app.UseMetricServer();
        app.UseRouting();
        app.UseHttpMetrics();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapMetrics("_system/metrics");
            endpoints.MapHealthChecks("/_system/health");
            endpoints.MapHealthChecks("/_system/ping", new HealthCheckOptions { Predicate = _ => false });
        });

        app.MapControllers();

        Log.Logger.Information("Starting Application");

        app.Run();
    }
}