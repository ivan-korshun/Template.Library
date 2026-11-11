// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Template.Library.Common.Http.HealthChecks;

public interface IHttpEndpointChecker
{
    Task<HealthCheckResult> CheckPublicAsync(Uri endpoint, CancellationToken token = default);

    Task<HealthCheckResult> CheckPublicAsync(
        Uri endpoint, HealthStatus onFailureStatus, CancellationToken token = default);
}
