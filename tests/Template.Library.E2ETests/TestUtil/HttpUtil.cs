// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Timeout;
using Template.Library.E2ETests;

namespace Template.Library.E2ETests.TestUtil;

public static class HttpUtil
{
    private static readonly ResiliencePipeline<HttpResponseMessage> s_retryPipeline = BuildRetryPipeline();
    private static readonly ResiliencePipeline<HttpResponseMessage> s_noRetryPipeline =
        new ResiliencePipelineBuilder<HttpResponseMessage>().Build();

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The handler is disposed by HttpClient")]
    public static HttpClient CreateClient(string? url = null, string? accessToken = null)
    {
        var client = new HttpClient(new ResilienceHandler(
            request => request.Method == HttpMethod.Post ? s_noRetryPipeline : s_retryPipeline)
        {
            InnerHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(4),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                ConnectTimeout = TimeSpan.FromSeconds(10)
            }
        });

        if (!string.IsNullOrEmpty(url))
        {
            client.BaseAddress = new Uri(url);
        }

        if (!string.IsNullOrEmpty(accessToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        AssemblyName assemblyName = Assembly.GetCallingAssembly().GetName();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            $"{assemblyName.Name!.Replace(".", " ")}/{assemblyName.Version!.ToString(3)}");

        return client;
    }

    public static async Task<string> GetAccessToken(HttpClient client)
    {
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = Settings.ClientCredentials.ClientId,
            ["client_secret"] = Settings.ClientCredentials.ClientSecret,
            ["audience"] = Settings.ClientCredentials.Audience
        });

        using HttpResponseMessage response = await client.PostAsync(Settings.ClientCredentials.TokenEndpoint, content);

        TokenResponse body = response.Should().HaveSuccessStatusCode().ContentAs<TokenResponse>();
        body.AccessToken.Should().NotBeNull();

        return body.AccessToken;
    }

    private static ResiliencePipeline<HttpResponseMessage> BuildRetryPipeline()
    {
        return new ResiliencePipelineBuilder<HttpResponseMessage>()
            .AddRetry(new HttpRetryStrategyOptions
            {
                // Tests should report non-transient 5XX errors instead of retrying them
                ShouldHandle = static args => args.Outcome switch
                    {
                        { Exception: HttpRequestException or TimeoutRejectedException } => PredicateResult.True(),
                        {
                            Result.StatusCode:
                                HttpStatusCode.RequestTimeout
                                or HttpStatusCode.TooManyRequests
                                or HttpStatusCode.BadGateway
                                or HttpStatusCode.ServiceUnavailable
                                or HttpStatusCode.GatewayTimeout
                                // Cloudflare custom codes: https://developers.cloudflare.com/support/troubleshooting/cloudflare-errors/troubleshooting-cloudflare-5xx-errors
                                or (HttpStatusCode)522 // Connection timed out
                                or (HttpStatusCode)524 // A timeout occurred
                                or (HttpStatusCode)527 // Railgun Listener to origin error
                        } => PredicateResult.True(),
                        _ => PredicateResult.False()
                    }
            })
            .Build();
    }

    private record TokenResponse([property: JsonPropertyName("access_token")] string? AccessToken);
}
