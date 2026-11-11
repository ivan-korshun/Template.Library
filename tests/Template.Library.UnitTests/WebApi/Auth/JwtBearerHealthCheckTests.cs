// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using System.Net.Http;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Template.Library.WebApi.Auth;

namespace Template.Library.UnitTests.WebApi.Auth;

#pragma warning disable CA1001 // HttpClient is disposed in TearDown
[TestFixture]
public class JwtBearerHealthCheckTests
{
    private TestableHttpMessageHandler _messageHandler = null!;
    private HttpClient _httpClient = null!;
    private OpenIdConnectConfiguration _openIdConfiguration = null!;
    private IConfigurationManager<OpenIdConnectConfiguration> _configurationManager = null!;
    private JwtBearerOptions _options = null!;
    private JwtBearerHealthCheck _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _messageHandler = Substitute.ForPartsOf<TestableHttpMessageHandler>();
        _httpClient = new HttpClient(_messageHandler);

        _openIdConfiguration = new OpenIdConnectConfiguration();
        _configurationManager = Substitute.For<IConfigurationManager<OpenIdConnectConfiguration>>();
        _configurationManager.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns(_openIdConfiguration);

        _options = new JwtBearerOptions
        {
            MetadataAddress = "https://idp.test/.well-known/openid-configuration",
            ConfigurationManager = _configurationManager
        };

        _sut = new JwtBearerHealthCheck(_httpClient, _options);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    [Test]
    public void Constructor_ConfigurationManagerIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        _options.ConfigurationManager = null;

        // Act
        Action action = () => _ = new JwtBearerHealthCheck(_httpClient, _options);

        // Assert
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("'options.ConfigurationManager' must not be null.");
    }

    [Test]
    public void Constructor_MetadataAddressIsNotAbsolute_ThrowsUriFormatException()
    {
        // Arrange
        _options.MetadataAddress = "well-known/openid-configuration";

        // Act
        Action action = () => _ = new JwtBearerHealthCheck(_httpClient, _options);

        // Assert
        action.Should().Throw<UriFormatException>();
    }

    [TestCase(HttpStatusCode.OK)]
    [TestCase(HttpStatusCode.Accepted)]
    [TestCase(HttpStatusCode.NoContent)]
    public async Task CheckHealthAsync_SigningKeysAvailableAndMetadataCallSuccessful_Healthy(HttpStatusCode statusCode)
    {
        // Arrange
        _openIdConfiguration.SigningKeys.Add(new SymmetricSecurityKey(new byte[32]));

        _messageHandler.DoSendAsync(
                Arg.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == new Uri(_options.MetadataAddress)),
                Arg.Any<CancellationToken>())
            .Returns(_ => new HttpResponseMessage(statusCode));

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().Be(HealthCheckResult.Healthy());
    }

    [Test]
    public async Task CheckHealthAsync_NoSigningKeys_Unhealthy()
    {
        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().Be(HealthCheckResult.Unhealthy("No public signing keys are available from the IdP."));

        _messageHandler.ReceivedCalls().Should().BeEmpty();
    }

    [Test]
    public async Task CheckHealthAsync_GetConfigurationAsyncThrows_Unhealthy()
    {
        // Arrange
        var exception = new InvalidOperationException("IdP metadata is not reachable.");

        _configurationManager.GetConfigurationAsync(Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().Be(HealthCheckResult.Unhealthy(
            "Failed to obtain public signing keys from the IdP.", exception));

        _messageHandler.ReceivedCalls().Should().BeEmpty();
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.Forbidden)]
    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task CheckHealthAsync_MetadataCallReturnsUnsuccessfulStatusCode_Degraded(HttpStatusCode statusCode)
    {
        // Arrange
        _openIdConfiguration.SigningKeys.Add(new SymmetricSecurityKey(new byte[32]));

        _messageHandler.DoSendAsync(
                Arg.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == new Uri(_options.MetadataAddress)),
                Arg.Any<CancellationToken>())
            .Returns(_ => new HttpResponseMessage(statusCode));

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().Be(HealthCheckResult.Degraded(
            $"Got {statusCode} response instead of success for {_options.MetadataAddress} request."));
    }

    [Test, AutoData]
    public async Task CheckHealthAsync_MetadataCallThrows_Degraded(HttpRequestException exception)
    {
        // Arrange
        _openIdConfiguration.SigningKeys.Add(new SymmetricSecurityKey(new byte[32]));

        _messageHandler.DoSendAsync(null!, CancellationToken.None).ThrowsForAnyArgs(exception);

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().Be(HealthCheckResult.Degraded(
            $"Exception occurred while checking {_options.MetadataAddress}: {exception.Message}",
            exception));
    }

    [Test]
    public async Task CheckHealthAsync_MetadataCallCancelled_Degraded()
    {
        // Arrange
        _openIdConfiguration.SigningKeys.Add(new SymmetricSecurityKey(new byte[32]));

        _messageHandler.DoSendAsync(
                Arg.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == new Uri(_options.MetadataAddress)),
                Arg.Any<CancellationToken>())
            .Returns(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(
            new HealthCheckContext(), cancellationTokenSource.Token);

        // Assert
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().StartWith($"Exception occurred while checking {_options.MetadataAddress}: ");
        result.Exception.Should().BeAssignableTo<OperationCanceledException>();
    }

    [Test]
    public async Task CheckHealthAsync_CancellationTokenSupplied_TokenIsPassedToConfigurationManager()
    {
        // Arrange
        _openIdConfiguration.SigningKeys.Add(new SymmetricSecurityKey(new byte[32]));

        _messageHandler.DoSendAsync(
                Arg.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == new Uri(_options.MetadataAddress)),
                Arg.Any<CancellationToken>())
            .Returns(_ => new HttpResponseMessage(HttpStatusCode.OK));

        using var cancellationTokenSource = new CancellationTokenSource();

        // Act
        HealthCheckResult result = await _sut.CheckHealthAsync(
            new HealthCheckContext(), cancellationTokenSource.Token);

        // Assert
        result.Should().Be(HealthCheckResult.Healthy());

        await _configurationManager.Received(1).GetConfigurationAsync(cancellationTokenSource.Token);
    }

    [Test]
    public async Task CheckHealthAsync_CancellationTokenSupplied_TokenIsLinkedToMetadataRequest()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();

        bool cancellationReachedRequest = false;

        _openIdConfiguration.SigningKeys.Add(new SymmetricSecurityKey(new byte[32]));

        _messageHandler.DoSendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                CancellationToken requestToken = callInfo.Arg<CancellationToken>();

                // The request token is a token linked to the supplied one, so it must observe its cancellation.
                // ReSharper disable once AccessToDisposedClosure
                cancellationTokenSource.Cancel();
                cancellationReachedRequest = requestToken.IsCancellationRequested;

                return new HttpResponseMessage(HttpStatusCode.OK);
            });

        // Act
        await _sut.CheckHealthAsync(new HealthCheckContext(), cancellationTokenSource.Token);

        // Assert
        cancellationReachedRequest.Should().BeTrue();
    }
}
