// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Template.Library.DataSource.Abstraction;

namespace Template.Library.DataSource.Organization;

public sealed class OrganizationServiceHealthCheck(IBranchRepository repository) : IHealthCheck
{
    private readonly IBranchRepository _repository = repository;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.GetAsync(int.MaxValue, cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(
                $"Exception occurred while checking Organization service: {e.Message}", e);
        }
    }
}
