// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Template.Library.Common.Http.ClientCredentials;
using Template.Library.Common.Http.Metrics;

namespace Template.Library.Common.Http;

public static class HttpServiceCollectionExtensions
{
    public static IServiceCollection AddHttpClientBasics(
        this IServiceCollection services, IConfigurationSection resilienceSection, string userAgent)
    {
        services.AddSingleton<IRetryMetrics, RetryMetrics>();
        services.AddSingleton<RetryMetricsCollector>();

        services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            // retries: HttpRequestException, TimeoutRejectedException, 408, 429, 500-599
            clientBuilder.AddStandardResilienceHandler(resilienceSection)
                .Configure((options, sp) =>
                {
                    options.Retry.DisableFor(HttpMethod.Post, HttpMethod.Delete, HttpMethod.Connect);
                    options.Retry.OnRetry = sp.GetRequiredService<RetryMetricsCollector>().OnRetry;
                });

            clientBuilder.ConfigureHttpClient(c => c.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent));
        });

        services.AddStaticHttpClient(HttpClientNames.DefaultStaticClient);

        return services;
    }

    public static IHttpClientBuilder AddAuthorizedHttpClient<TOption>(this IServiceCollection services, string name)
        where TOption : ServiceOptionsBase
    {
        return services
            .AddHttpClient(name, (sp, c) =>
            {
                TOption options = sp.GetRequiredService<IOptions<TOption>>().Value;
                c.BaseAddress = new Uri(options.Host.TrimEnd('/') + "/");
            })
            .ConfigureAsStatic()
            .AddAuthorizationHandler<TOption>();
    }

    public static IHttpClientBuilder AddStaticHttpClient(this IServiceCollection services, string name)
    {
        return services
            .AddHttpClient(name)
            .ConfigureAsStatic();
    }

    private static IHttpClientBuilder ConfigureAsStatic(this IHttpClientBuilder builder)
    {
        return builder
            /* Recreate connection every N minutes */
            .UseSocketsHttpHandler((handler, _) => handler.PooledConnectionLifetime = TimeSpan.FromMinutes(2))
            /* Disable rotation, as it is handled by PooledConnectionLifetime */
            .SetHandlerLifetime(Timeout.InfiniteTimeSpan);
    }

    private static IHttpClientBuilder AddAuthorizationHandler<TOption>(this IHttpClientBuilder builder)
        where TOption : ServiceOptionsBase
    {
        return builder.AddHttpMessageHandler(provider =>
        {
            TOption options = provider.GetRequiredService<IOptions<TOption>>().Value;
            var tokenManager = provider.GetRequiredKeyedService<IAccessTokenManager>(options.ClientCredentials);
            return new AccessTokenHandler(tokenManager);
        });
    }
}
