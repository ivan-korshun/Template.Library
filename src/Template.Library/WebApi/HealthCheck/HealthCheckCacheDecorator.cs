// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Template.Library.WebApi.HealthCheck;

public sealed class HealthCheckCacheDecorator<T>(T innerCheck, IOptions<HealthChecksOptions> options)
    : IHealthCheck
    where T : IHealthCheck
{
    private readonly T _innerCheck = innerCheck;
    private readonly HealthChecksOptions _options = options.Value;

    private HealthCheckResult _lastResult = HealthCheckResult.Healthy();
    private DateTime _lastCheck = DateTime.MinValue;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (DateTime.UtcNow < _lastCheck.Add(_options.CachedResultExpiration))
        {
            return _lastResult;
        }

        HealthCheckResult newResult = await _innerCheck.CheckHealthAsync(context, cancellationToken);
        _lastResult = newResult;
        _lastCheck = DateTime.UtcNow;

        return newResult;
    }
}
