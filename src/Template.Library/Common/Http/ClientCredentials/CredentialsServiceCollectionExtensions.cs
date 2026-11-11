// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Template.Library.Common.Http.ClientCredentials.Models;

namespace Template.Library.Common.Http.ClientCredentials;

public static class CredentialsServiceCollectionExtensions
{
    public static IServiceCollection AddAccessTokenManagers(
        this IServiceCollection services, ClientCredentialsOptions clientCredentials)
    {
        services.AddSingleton<ITokenProvider, TokenProvider>();

        foreach ((string name, TokenRequest? tokenRequest) in clientCredentials)
        {
            services.AddKeyedSingleton<IAccessTokenManager>(name, (sp, _) => new AccessTokenManager(
                sp.GetRequiredService<ITokenProvider>(), tokenRequest));
        }

        return services;
    }
}
