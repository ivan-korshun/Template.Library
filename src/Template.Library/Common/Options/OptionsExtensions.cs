// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Template.Library.Common.Options;

public static class OptionsExtensions
{
    public static IServiceCollection AddValidOptions<TOptions>(this IServiceCollection services, IConfiguration config)
        where TOptions : class, IOptionsBase
    {
        services.AddOptions<TOptions>()
            .Bind(config.GetSection(TOptions.Key))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddValidOptions<TOptions>(this IServiceCollection services, IConfigurationSection section)
        where TOptions : class
    {
        services.AddOptions<TOptions>()
            .Bind(section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static TOptions GetValidOptions<TOptions>(this IConfiguration config)
        where TOptions : class, IOptionsBase
    {
        TOptions options = config.GetSection(TOptions.Key).Get<TOptions>().EnsureNotNull();
        Validator.ValidateObject(options, new ValidationContext(options), validateAllProperties: true);
        return options;
    }
}
