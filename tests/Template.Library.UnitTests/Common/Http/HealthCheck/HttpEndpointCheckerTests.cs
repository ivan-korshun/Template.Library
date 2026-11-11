// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using System.Net.Http;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Template.Library.Common.Http;
using Template.Library.Common.Http.HealthChecks;

namespace Template.Library.UnitTests.Common.Http.HealthCheck;

#pragma warning disable CA1001 // HttpClient is disposed in TearDown
#pragma warning disable CA2000 // Created HttpResponseMessage is disposed inside HttpEndpointChecker

[TestFixture]
public class HttpEndpointCheckerTests
{
    private readonly Uri _endpoint = new("http://test/endpoint");

    private TestableHttpMessageHandler _messageHandler = null!;
    private HttpClient _httpClient = null!;
    private HttpEndpointChecker _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _messageHandler = Substitute.ForPartsOf<TestableHttpMessageHandler>();
        _httpClient = new HttpClient(_messageHandler);

        var clientFactory = Substitute.For<IHttpClientFactory>();
        clientFactory.CreateClient(HttpClientNames.DefaultStaticClient).Returns(_httpClient);

        _sut = new HttpEndpointChecker(clientFactory);
    }

    [TearDown]
    public void Teardown()
    {
        _httpClient.Dispose();
    }

    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.Forbidden)]
    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.InternalServerError)]
    public async Task CheckPublicAsync_WrongResponseFromEndpoint_Unhealthy(HttpStatusCode statusCode)
    {
        // Arrange
        _messageHandler.DoSendAsync(
                Arg.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == _endpoint),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(statusCode));

        // Act
        HealthCheckResult result = await _sut.CheckPublicAsync(_endpoint);

        // Assert
        result.Should().Be(HealthCheckResult.Unhealthy(
            $"Got {statusCode} response instead of success for {_endpoint} request."));
    }

    [TestCase(HttpStatusCode.OK)]
    [TestCase(HttpStatusCode.Accepted)]
    [TestCase(HttpStatusCode.NoContent)]
    public async Task CheckPublicAsync_2XXFromEndpoint_Healthy(HttpStatusCode statusCode)
    {
        // Arrange
        _messageHandler.DoSendAsync(
                Arg.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == _endpoint),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(statusCode));

        // Act
        HealthCheckResult result = await _sut.CheckPublicAsync(_endpoint);

        // Assert
        result.Should().Be(HealthCheckResult.Healthy());
    }

    [Test, AutoData]
    public async Task CheckPublicAsync_ExceptionFromEndpoint_Unhealthy(HttpRequestException exception)
    {
        // Arrange
        _messageHandler.DoSendAsync(null!, CancellationToken.None).ThrowsForAnyArgs(exception);

        // Act
        HealthCheckResult result = await _sut.CheckPublicAsync(_endpoint);

        // Assert
        result.Should().Be(HealthCheckResult.Unhealthy(
            $"Exception occurred while checking {_endpoint}: {exception.Message}", exception));
    }

    [Test]
    [InlineAutoData(HttpStatusCode.BadRequest)]
    [InlineAutoData(HttpStatusCode.NotFound)]
    [InlineAutoData(HttpStatusCode.InternalServerError)]
    public async Task CheckPublicAsyncWithNegativeStatus_WrongResponseFromEndpoint_NegativeStatus(
        HttpStatusCode statusCode, HealthStatus negativeStatus)
    {
        // Arrange
        _messageHandler.DoSendAsync(
                Arg.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == _endpoint),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(statusCode));

        // Act
        HealthCheckResult result = await _sut.CheckPublicAsync(_endpoint, negativeStatus);

        // Assert
        result.Should().Be(new HealthCheckResult(
            negativeStatus, $"Got {statusCode} response instead of success for {_endpoint} request."));
    }

    [Test, AutoData]
    public async Task CheckPublicAsyncWithNegativeStatus_2XXFromEndpoint_Healthy(HealthStatus negativeStatus)
    {
        // Arrange
        _messageHandler.DoSendAsync(
                Arg.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get && r.RequestUri == _endpoint),
                Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(HttpStatusCode.OK));

        // Act
        HealthCheckResult result = await _sut.CheckPublicAsync(_endpoint, negativeStatus);

        // Assert
        result.Should().Be(HealthCheckResult.Healthy());
    }

    [Test, AutoData]
    public async Task CheckPublicAsyncWithNegativeStatus_ExceptionFromEndpoint_NegativeStatus(
        HttpRequestException exception, HealthStatus negativeStatus)
    {
        // Arrange
        _messageHandler.DoSendAsync(null!, CancellationToken.None).ThrowsForAnyArgs(exception);

        // Act
        HealthCheckResult result = await _sut.CheckPublicAsync(_endpoint, negativeStatus);

        // Assert
        result.Should().Be(new HealthCheckResult(
            negativeStatus, $"Exception occurred while checking {_endpoint}: {exception.Message}", exception));
    }
}
