// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using AutoFixture.NUnit4;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Template.Library.Common.Http;
using Template.Library.Common.Http.ClientCredentials;
using Template.Library.Common.Http.ClientCredentials.Models;

namespace Template.Library.UnitTests.Common.Http.ClientCredentials;

#pragma warning disable CA1001 // HttpClient is disposed in TearDown
[TestFixture]
public class TokenProviderTests
{
    private TestableHttpMessageHandler _messageHandler = null!;
    private HttpClient _httpClient = null!;
    private TokenProvider _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _messageHandler = Substitute.ForPartsOf<TestableHttpMessageHandler>();
        _httpClient = new HttpClient(_messageHandler);

        var clientFactory = Substitute.For<IHttpClientFactory>();
        clientFactory.CreateClient(HttpClientNames.DefaultStaticClient).Returns(_httpClient);

        _sut = new TokenProvider(clientFactory);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    [Test, AutoData]
    public async Task GetTokenAsync_WithAudience_ReturnsTokenResponse(
        Uri tokenEndpoint, string clientId, string clientSecret, string audience, TokenResponse tokenResponse)
    {
        // Arrange
        var request = new TokenRequest
        {
            TokenEndpoint = tokenEndpoint.ToString(),
            ClientId = clientId,
            ClientSecret = clientSecret,
            Audience = audience
        };

        string? capturedBody = null;

        _messageHandler.DoSendAsync(
                Arg.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post && r.RequestUri == tokenEndpoint),
                Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                capturedBody = await ci.Arg<HttpRequestMessage>().Content!.ReadAsStringAsync();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(tokenResponse)
                };
            });

        // Act
        TokenResponse result = await _sut.GetTokenAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(tokenResponse);
        capturedBody.Should().Be(
            $"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}&audience={audience}");
    }

    [Test, AutoData]
    public async Task GetTokenAsync_WithoutAudience_ReturnsTokenResponse(
        Uri tokenEndpoint, string clientId, string clientSecret, TokenResponse tokenResponse)
    {
        // Arrange
        var request = new TokenRequest
        {
            TokenEndpoint = tokenEndpoint.ToString(),
            ClientId = clientId,
            ClientSecret = clientSecret,
            Audience = null
        };

        string? capturedBody = null;

        _messageHandler.DoSendAsync(
                Arg.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post && r.RequestUri == tokenEndpoint),
                Arg.Any<CancellationToken>())
            .Returns(async ci =>
            {
                capturedBody = await ci.Arg<HttpRequestMessage>().Content!.ReadAsStringAsync();
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(tokenResponse)
                };
            });

        // Act
        TokenResponse result = await _sut.GetTokenAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(tokenResponse);
        capturedBody.Should().Be($"grant_type=client_credentials&client_id={clientId}&client_secret={clientSecret}");
    }

    [Test]
    [InlineAutoData(HttpStatusCode.BadRequest)]
    [InlineAutoData(HttpStatusCode.Unauthorized)]
    [InlineAutoData(HttpStatusCode.Forbidden)]
    [InlineAutoData(HttpStatusCode.InternalServerError)]
    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed in SUT")]
    public async Task GetTokenAsync_NonSuccessStatusCode_HttpRequestException(
        HttpStatusCode statusCode, Uri tokenEndpoint, string clientId, string clientSecret, string responseResult)
    {
        // Arrange
        var request = new TokenRequest
        {
            TokenEndpoint = tokenEndpoint.ToString(),
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        _messageHandler.DoSendAsync(
                Arg.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post && r.RequestUri == tokenEndpoint),
                Arg.Any<CancellationToken>())
            .Returns(ci => new HttpResponseMessage(statusCode)
            {
                RequestMessage = ci.Arg<HttpRequestMessage>(),
                Content = JsonContent.Create(responseResult)
            });

        // Act
        Func<Task> act = () => _sut.GetTokenAsync(request, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<HttpOperationException>())
            .WithMessage($"POST {tokenEndpoint} has failed with {statusCode} code. Content: \"{responseResult}\".");
    }
}
