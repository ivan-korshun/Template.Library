// Copyright © Korshun Ivan. All rights reserved.

using AutoFixture;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Template.Library.WebApi.HealthCheck;

namespace Template.Library.UnitTests.WebApi.HealthCheck;

public class HealthCheckCacheDecoratorTests
{
    private IHealthCheck _innerCheck = null!;
    private HealthChecksOptions _options = null!;
    private HealthCheckCacheDecorator<IHealthCheck> _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _innerCheck = Substitute.For<IHealthCheck>();

        _options = new Fixture().Create<HealthChecksOptions>();
        _options.CachedResultExpiration = TimeSpan.FromSeconds(5);

        _sut = new HealthCheckCacheDecorator<IHealthCheck>(
            _innerCheck, new OptionsWrapper<HealthChecksOptions>(_options));
    }

    [Test, AutoData]
    public async Task CheckHealthAsync_CacheIsEmpty_PerformsCheck(
        HealthCheckResult innerResult, CancellationToken token)
    {
        // Arrange
        var context = new HealthCheckContext();
        _innerCheck.CheckHealthAsync(context, token).Returns(innerResult);

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(context, token);

        // Assert
        result.Should().Be(innerResult);
    }

    [Test, AutoData]
    public async Task CheckHealthAsync_CacheIsExpired_PerformsCheck(
        HealthCheckResult firstResult, HealthCheckResult secondResult, CancellationToken token)
    {
        // Arrange
        _options.CachedResultExpiration = TimeSpan.Zero;

        var context = new HealthCheckContext();
        _innerCheck.CheckHealthAsync(context, token).Returns(firstResult, secondResult);

        _ = await _sut.CheckHealthAsync(context, token);

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(context, token);

        // Assert
        result.Should().Be(secondResult);
    }

    [Test]
    [InlineAutoData(HealthStatus.Healthy)]
    [InlineAutoData(HealthStatus.Degraded)]
    [InlineAutoData(HealthStatus.Unhealthy)]
    public async Task CheckHealthAsync_CacheIsValid_ReturnsCachedValue(
        HealthStatus firstStatus, HealthCheckResult secondResult, CancellationToken token)
    {
        // Arrange
        var firstResult = new HealthCheckResult(firstStatus);

        var context = new HealthCheckContext();
        _innerCheck.CheckHealthAsync(context, token).Returns(firstResult, secondResult);

        _ = await _sut.CheckHealthAsync(context, token);

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(context, token);

        // Assert
        result.Should().Be(firstResult);
    }
}
