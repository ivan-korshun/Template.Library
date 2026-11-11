// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Template.Library.WebApi.HealthCheck;

public static class HealthCheckServiceCollectionExtensions
{
    public static IHealthChecksBuilder AddCachedHealthCheck<T>(
        this IHealthChecksBuilder builder,
        string name,
        ICollection<string> tags,
        Func<IServiceProvider, T>? factory = null)
        where T : class, IHealthCheck
    {
        if (factory != null)
        {
            builder.Services.AddSingleton(factory);
        }
        else
        {
            builder.Services.AddSingleton<T>();
        }

        builder.Services.AddSingleton<HealthCheckCacheDecorator<T>>();

        builder.AddCheck<HealthCheckCacheDecorator<T>>(name, HealthStatus.Unhealthy, tags);

        return builder;
    }
}
