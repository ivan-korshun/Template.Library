// Copyright © Korshun Ivan. All rights reserved.

using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Template.Library.Common.Http.ClientCredentials;
using Template.Library.Common.Http.ClientCredentials.Models;

namespace Template.Library.UnitTests.Common.Http.ClientCredentials;

[TestFixture]
public class CredentialsServiceCollectionExtensionsTests
{
    [Test, AutoData]
    public async Task AddAccessTokenManagers_Always_RegisterTokenManagersWithTokenProvider(
        Dictionary<string, TokenRequest> tokenRequests)
    {
        // Arrange
        var options = new ClientCredentialsOptions();
        foreach (var pair in tokenRequests)
        {
            options.Add(pair.Key, pair.Value);
        }

        var services = new ServiceCollection();

        // Act
        services.AddAccessTokenManagers(options);

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        services.Should().Contain(x => x.ServiceType == typeof(ITokenProvider)
            && x.Lifetime == ServiceLifetime.Singleton
            && x.ImplementationType == typeof(TokenProvider));

        foreach (var tokenSource in tokenRequests)
        {
            services.Should().Contain(x => x.ServiceType == typeof(IAccessTokenManager)
                && x.Lifetime == ServiceLifetime.Singleton
                && Equals(x.ServiceKey, tokenSource.Key));
        }
    }
}
