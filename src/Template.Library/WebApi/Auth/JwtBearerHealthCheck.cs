// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Template.Library.Common;

namespace Template.Library.WebApi.Auth;

public class JwtBearerHealthCheck(HttpClient httpClient, JwtBearerOptions options) : IHealthCheck
{
    private readonly Uri _metadataAddress = new(options.MetadataAddress, UriKind.Absolute);

    private readonly IConfigurationManager<OpenIdConnectConfiguration> _configurationManager =
        options.ConfigurationManager.EnsureNotNull();

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            OpenIdConnectConfiguration configuration = await _configurationManager.GetConfigurationAsync(cancellationToken);
            if (configuration.SigningKeys.Count == 0)
            {
                return HealthCheckResult.Unhealthy("No public signing keys are available from the IdP.");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to obtain public signing keys from the IdP.", ex);
        }

        try
        {
            using HttpResponseMessage response = await httpClient.GetAsync(_metadataAddress, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new HealthCheckResult(
                    HealthStatus.Degraded,
                    $"Got {response.StatusCode} response instead of success for {_metadataAddress} request.");
            }
        }
        catch (Exception e)
        {
            return new HealthCheckResult(
                HealthStatus.Degraded, $"Exception occurred while checking {_metadataAddress}: {e.Message}", e);
        }

        return HealthCheckResult.Healthy();
    }
}
