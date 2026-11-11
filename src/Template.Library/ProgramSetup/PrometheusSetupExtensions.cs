// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Prometheus;
using Prometheus.DotNetRuntime;
using Template.Library.Common.Options;

namespace Template.Library.ProgramSetup;

[ExcludeFromCodeCoverage]
public static class PrometheusSetupExtensions
{
    private static IDisposable? s_collector;

    public static void StartCollectingMetrics(this DotNetRuntimeStatsBuilder.Builder builder)
    {
        s_collector ??= builder
            .WithContentionStats()
            .WithGcStats()
            .WithJitStats()
            .WithThreadPoolStats()
            .WithSocketStats()
            .WithErrorHandler(ex => Console.WriteLine("ERROR: " + ex))
#if DEBUG
            .WithDebuggingMetrics(true)
#endif
            .StartCollecting();
    }

    public static IEndpointConventionBuilder MapAppMetrics(this IEndpointRouteBuilder endpoints, IConfiguration config)
    {
        var options = config.GetValidOptions<MetricsOptions>();

        IEndpointConventionBuilder mapMetrics = endpoints.MapMetrics(options.ExporterEndpoint);

        if (options.ExporterPort.HasValue)
        {
            mapMetrics.RequireHost($"*:{options.ExporterPort}");
        }

        return mapMetrics;
    }

    private sealed class MetricsOptions : IOptionsBase
    {
        public static string Key => "Metrics";

        /// <summary>
        /// Gets or sets the route at which to make the metrics available.
        /// </summary>
        public required string ExporterEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the port on which to expose the metrics route.
        /// </summary>
        /// <remarks>The default value of 0 is treated as no port being set and will expose the route on all ports.</remarks>
        public int? ExporterPort { get; set; }
    }
}
