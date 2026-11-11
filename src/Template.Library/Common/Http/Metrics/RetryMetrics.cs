// Copyright © Korshun Ivan. All rights reserved.

using Prometheus;

namespace Template.Library.Common.Http.Metrics;

public sealed class RetryMetrics : IRetryMetrics
{
    public ICollector<ICounter> RetryTotalCount { get; } = Prometheus.Metrics.CreateCounter(
        "retry_count_total",
        "The total number of retries by target and status.",
        "target", "status");
}
