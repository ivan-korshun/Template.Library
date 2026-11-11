// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Library.Common;

namespace Template.Library.ProgramSetup;

[ExcludeFromCodeCoverage]
public static class CorsSetupExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfiguration config)
    {
        var corsOptions = config.GetSection("CORS").Get<Dictionary<string, CorsPolicyOptions>>().EnsureNotNull();
        if (corsOptions.Count == 0)
        {
            return services;
        }

        services.AddCors(options =>
        {
            foreach ((string name, CorsPolicyOptions policyOptions) in corsOptions)
            {
                options.AddPolicy(name, builder =>
                {
                    if (!string.IsNullOrEmpty(policyOptions.AllowedOrigins))
                    {
                        var allowedOrigins = policyOptions.AllowedOrigins.SplitWithTrim().ToArray();

                        builder.WithOrigins(allowedOrigins)
                            .SetIsOriginAllowedToAllowWildcardSubdomains();
                    }

                    if (!string.IsNullOrEmpty(policyOptions.AllowedMethods))
                    {
                        var allowedMethods = policyOptions.AllowedMethods.SplitWithTrim().ToArray();
                        builder.WithMethods(allowedMethods);
                    }

                    if (!string.IsNullOrEmpty(policyOptions.AllowedHeaders))
                    {
                        var allowedHeaders = policyOptions.AllowedHeaders.SplitWithTrim().ToArray();
                        builder.WithHeaders(allowedHeaders);
                    }
                });
            }
        });

        return services;
    }

    private sealed record CorsPolicyOptions(string? AllowedOrigins, string? AllowedMethods, string? AllowedHeaders);
}
