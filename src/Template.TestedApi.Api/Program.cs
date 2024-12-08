using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Serilog;
using System;
using System.Linq;
using Template.TestedApi.Api.HostedServices;
using Template.TestedApi.Api.Middleware;

namespace Template.TestedApi.Api;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.WithSerilog(builder.Configuration, "Template.TestedApi API");
            builder.Services.WithPostgres(builder.Configuration);
            builder.Services.WithOpenIdConnect(builder.Configuration);
            builder.Services.WithMediatr();

            builder.Services.AddAuthorization(authorizationOptions => {

                authorizationOptions.AddPolicy("TodoList:Read", policy => policy.RequireClaim("permission", "TodoList:Read"));
                authorizationOptions.AddPolicy("TodoList:Write", policy => policy.RequireClaim("permission", "TodoList:Read"));

            });

            builder.Services
                .AddHealthChecks()
                .AddNpgSql(s =>
                {
                    return builder.Configuration.GetConnectionString("PostgresDb");
                });

            // TODO: Config cleanup
            builder.Services.AddHostedService<PostgresInitService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<AuthMiddleware>();

            app.UseHttpsRedirectionExcluding("/_system");

            // https://github.com/prometheus-net/prometheus-net
            app.UseMetricServer();

            app.UseRouting();
            app.UseHttpMetrics();
            app.UseAuthorization();
            app.MapControllers();
            app.MapMetrics("_system/metrics");
            app.MapHealthChecks("/_system/ping", new HealthCheckOptions { Predicate = _ => false });
            app.MapHealthChecks("/_system/health", new HealthCheckOptions
            {
                ResponseWriter = async (c, r) =>
                {
                    var response = new
                    {
                        Status = r.Status.ToString(),
                        Checks = r.Entries.Select(x =>
                            new
                            {
                                Status = x.Value.Status.ToString(),
                                Component = x.Key,
                                Description = x.Value.Description
                            }),
                        Duration = r.TotalDuration
                    };
                    await c.Response.WriteAsJsonAsync(response);
                }
            });

            Log.Logger.Information("Starting Application");

            app.Run();
        }
        catch(Exception ex)
        {
            Log.Logger.Error(ex, "Error Starting Application");
        }
    }
}
