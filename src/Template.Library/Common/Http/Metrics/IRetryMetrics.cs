// Copyright © Korshun Ivan. All rights reserved.

using Prometheus;

namespace Template.Library.Common.Http.Metrics;

public interface IRetryMetrics
{
    ICollector<ICounter> RetryTotalCount { get; }
}
