// Copyright © Korshun Ivan. All rights reserved.

using AutoFixture;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Template.Library.Common.Http.ClientCredentials;

namespace Template.Library.UnitTests.Common.Http.ClientCredentials;

public class CredentialsHealthCheckTests
{
    private CredentialsHealthCheck _sut = null!;
    private List<IAccessTokenManager> _tokenManagers = null!;

    [SetUp]
    public void SetUp()
    {
        var fixture = new Fixture();

        _tokenManagers = [Substitute.For<IAccessTokenManager>(), Substitute.For<IAccessTokenManager>()];
        foreach (var tokenManager in _tokenManagers)
        {
            tokenManager.TokenEndpoint.Returns(fixture.Create<string>());
        }

        _sut = new CredentialsHealthCheck(_tokenManagers);
    }

    [Test, AutoData]
    public async Task CheckHealthAsync_AllManagersReturnsToken_Healthy(string accessToken, CancellationToken ct)
    {
        // Arrange
        foreach (IAccessTokenManager manager in _tokenManagers)
        {
            manager.GetTokenAsync(ct).Returns(accessToken);
        }

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(new HealthCheckContext(), ct);

        // Assert
        result.Should().Be(HealthCheckResult.Healthy());
    }

    [Test, AutoData]
    public async Task CheckHealthAsync_SecondManagerReturnsEmptyToken_Unhealthy(string accessToken)
    {
        // Arrange
        _tokenManagers[0].GetTokenAsync(CancellationToken.None).Returns(accessToken);
        _tokenManagers[1].GetTokenAsync(CancellationToken.None).Returns(string.Empty);

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        result.Should().Be(HealthCheckResult.Unhealthy(
            $"Failed to retrieve token from '{_tokenManagers[1].TokenEndpoint}'."));
    }

    [Test, AutoData]
    public async Task CheckHealthAsync_FirstManagerThrowsException_Unhealthy(Exception exception, string accessToken)
    {
        // Arrange
        _tokenManagers[0].GetTokenAsync(CancellationToken.None).Throws(exception);
        _tokenManagers[1].GetTokenAsync(CancellationToken.None).Returns(accessToken);

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

        // Assert
        result.Should().Be(HealthCheckResult.Unhealthy(
            $"Failed to retrieve token from '{_tokenManagers[0].TokenEndpoint}': {exception.Message}", exception));
    }
}
