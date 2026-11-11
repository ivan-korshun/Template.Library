// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Template.Library.Common.Http.HealthChecks;

public class HttpEndpointChecker(IHttpClientFactory httpClientFactory) : IHttpEndpointChecker
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientNames.DefaultStaticClient);

    public Task<HealthCheckResult> CheckPublicAsync(Uri endpoint, CancellationToken token = default)
    {
        return CheckPublicAsync(endpoint, HealthStatus.Unhealthy, token);
    }

    public async Task<HealthCheckResult> CheckPublicAsync(
        Uri endpoint, HealthStatus onFailureStatus, CancellationToken token = default)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync(endpoint, token);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : new HealthCheckResult(
                    onFailureStatus, $"Got {response.StatusCode} response instead of success for {endpoint} request.");
        }
        catch (Exception e)
        {
            return new HealthCheckResult(
                onFailureStatus, $"Exception occurred while checking {endpoint}: {e.Message}", e);
        }
    }
}
