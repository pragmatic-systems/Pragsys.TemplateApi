using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Serilog;
using System;

namespace Template.TestedApi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.WithSerilog(builder.Configuration, "Template.TestedApi API");
            builder.Services.WithPostgres(builder.Configuration);
            builder.Services.WithMediatr();
            builder.Services.WithOpenIdConnect(builder.Configuration);
            builder.Services.WithAuthorizationPolicy();
            builder.Services.AddControllers();
            builder.Services.AddAppHealthChecks(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMetricServer(); // https://github.com/prometheus-net/prometheus-net
            app.UseAuthMiddleware();
            app.UseRouting();
            app.UseHttpMetrics();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapInstrumentationEndpoints();

            Log.Logger.Information("Starting Application");

            app.Run();
        }
        catch(Exception ex)
        {
            Log.Logger.Error(ex, "Error Starting Application");
        }
    }
}
