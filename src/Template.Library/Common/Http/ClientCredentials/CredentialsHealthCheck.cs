// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Template.Library.Common.Http.ClientCredentials;

public class CredentialsHealthCheck(IEnumerable<IAccessTokenManager> tokenManagers) : IHealthCheck
{
    private readonly List<IAccessTokenManager> _tokenManagers = [.. tokenManagers];

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken)
    {
        foreach (IAccessTokenManager manager in _tokenManagers)
        {
            try
            {
                string token = await manager.GetTokenAsync(cancellationToken);
                if (string.IsNullOrEmpty(token))
                {
                    return HealthCheckResult.Unhealthy($"Failed to retrieve token from '{manager.TokenEndpoint}'.");
                }
            }
            catch (Exception e)
            {
                return HealthCheckResult.Unhealthy(
                    $"Failed to retrieve token from '{manager.TokenEndpoint}': {e.Message}", e);
            }
        }

        return HealthCheckResult.Healthy();
    }
}
