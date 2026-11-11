// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Template.Library.Common;

namespace Template.Library.WebApi.HealthCheck;

public static class HealthCheckWebApplicationExtensions
{
    public static void MapHealthChecks(this WebApplication app)
    {
        HealthChecksOptions options = app.Services.GetRequiredService<IOptions<HealthChecksOptions>>().Value;
        string healthEndpoint = options.HealthEndpoint.Trim('/');

        app.MapHealthChecks(
            healthEndpoint,
            new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("health")
            });

        foreach (string tag in options.Tags)
        {
            app.MapHealthChecks(
                string.Concat(healthEndpoint, "/", tag.EnsureNotEmpty()),
                new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
                {
                    Predicate = registration => registration.Tags.Contains(tag)
                });
        }

        app.MapHealthChecks(
            options.StartedEndpoint,
            new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("started")
            });

        app.MapHealthChecks(
            options.ReadinessEndpoint,
            new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("ready")
            });

        app.MapHealthChecks(
            options.LivenessEndpoint,
            new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = registration => registration.Tags.Contains("live")
            });
    }
}
