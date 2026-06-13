using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Pragsys.TemplateApi.Api.Auth;
using Pragsys.TemplateApi.Instrumentation;

namespace Pragsys.TemplateApi.Api;

public static class ConfigurationExtensions
{
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
            })

            // Cookie-based auth scheme for the Hangfire dashboard browser UI.
            // Browsers don't send Authorization headers, so the login endpoint
            // validates a pasted JWT and creates a session cookie.
            .AddCookie("HangfireCookie", options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true;
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

    public static WebApplication ConfigureSwagger(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }

    public static WebApplication UseHangfireDashboard(this WebApplication app)
    {
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[]
            {
                new HangfireAuthorizationFilter(),
            },
            DisplayStorageConnectionString = false,
            AppPath = null,
        });

        return app;
    }

    public static WebApplication ConfigureAuthentication(this WebApplication app)
    {
        // Use authorization, excluding hangfire login paths.
        app.UseWhen(
                context => !context.IsHangfireLoginPath(),
                builder => builder.UseAuthorization());

        return app;
    }

    public static bool IsHangfireLoginPath(this HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/hangfire-login")
            || context.Request.Path.StartsWithSegments("/hangfire-logout");
    }

    public static WebApplication ConfigureHangfireSessionManagement(this WebApplication app)
    {
        // POST - validate JWT, create cookie session
        app.MapPost("/hangfire-login",
            async (
                HttpContext ctx,
                IConfiguration config,
                IClaimsTransformation claimsTransformer,
                IConfigurationManager<OpenIdConnectConfiguration> oidcConfigManager)
            =>
        {
            var token = ctx.Request.Form["token"].ToString();
            if (string.IsNullOrWhiteSpace(token))
                return Results.Unauthorized();

            try
            {
                var issuer = config["OpenIdConnect:Issuer"];
                var audience = config["OpenIdConnect:Audience"];

                var oidcConfig = await oidcConfigManager.GetConfigurationAsync(default);
                var handler = new JwtSecurityTokenHandler();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidIssuer = issuer,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = oidcConfig.SigningKeys,
                    AudienceValidator = TokenValidators.ValidateAudienceOrClientId,
                };

                var principal = handler.ValidateToken(token, validationParameters, out _);
                var transformedPrincipal = await claimsTransformer.TransformAsync(principal);

                if (!transformedPrincipal.HasClaim(ClaimTypes.Role, Roles.HangfireDashboard))
                    return Results.Forbid();

                await ctx.SignInAsync("HangfireCookie", transformedPrincipal);
                return Results.Redirect("/hangfire");
            }
            catch
            {
                return Results.Unauthorized();
            }
        });

        // POST - logout
        app.MapPost("/hangfire-logout", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync("HangfireCookie");
            return Results.Ok(new { message = "Logged out" });
        });

        return app;
    }
}
