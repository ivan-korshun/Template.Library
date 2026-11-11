// Copyright © Korshun Ivan. All rights reserved.

using System.Globalization;
using System.Net.Http;
using Polly;
using Polly.Retry;

namespace Template.Library.Common.Http.Metrics;

public sealed class RetryMetricsCollector(IRetryMetrics metrics)
{
    private readonly IRetryMetrics _metrics = metrics;

    public ValueTask OnRetry(OnRetryArguments<HttpResponseMessage> args)
    {
        string? host = args.Context.GetRequestMessage()?.RequestUri?.Host;
        string? statusCode = ((int?)args.Outcome.Result?.StatusCode)?.ToString(CultureInfo.InvariantCulture);

        _metrics.RetryTotalCount.WithLabels(host ?? "", statusCode ?? "").Inc();

        return ValueTask.CompletedTask;
    }
}
