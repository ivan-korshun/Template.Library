// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Template.Library.Common.Http.HealthChecks;

namespace Template.Library.DataSource.Organization;

public sealed class OrganizationServiceLifeCheck(
    IHttpEndpointChecker endpointChecker, IOptions<OrganizationServiceOptions> options) : IHealthCheck
{
    private readonly IHttpEndpointChecker _endpointChecker = endpointChecker;
    private readonly OrganizationServiceOptions _options = options.Value;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return _endpointChecker.CheckPublicAsync(
            new Uri($"{_options.Host}/api/status/healthz/live", UriKind.Absolute), cancellationToken);
    }
}
