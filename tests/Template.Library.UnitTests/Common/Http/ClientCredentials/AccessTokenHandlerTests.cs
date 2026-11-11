// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Template.Library.Common.Http.ClientCredentials;

namespace Template.Library.UnitTests.Common.Http.ClientCredentials;

#pragma warning disable CA1001 // Dispose happens in TearDown
[TestFixture]
public class AccessTokenHandlerTests
{
    private TestableHttpMessageHandler _innerHandler = null!;
    private IAccessTokenManager _tokenManager = null!;
    private HttpClient _client = null!;
    private AccessTokenHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _tokenManager = Substitute.For<IAccessTokenManager>();
        _innerHandler = Substitute.ForPartsOf<TestableHttpMessageHandler>();

        _sut = new AccessTokenHandler(_tokenManager)
        {
            InnerHandler = _innerHandler
        };

        _client = new HttpClient(_sut);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _sut.Dispose();
    }

    [Test, AutoNSubstituteData]
    public async Task SendAsync_Always_AppendsAuthorizationHeader(
        Uri uri, string accessToken, HttpResponseMessage response)
    {
        // Arrange
        _tokenManager.GetTokenAsync(Arg.Any<CancellationToken>()).Returns(accessToken);

        _innerHandler.DoSendAsync(
            Arg.Is<HttpRequestMessage>(
                r => r.RequestUri == uri && r.Headers.GetValues("Authorization").First() == $"Bearer {accessToken}"),
            Arg.Any<CancellationToken>())
            .Returns(response);

        // Act
        using HttpResponseMessage result = await _client.GetAsync(uri);

        // Assert
        result.StatusCode.Should().Be(response.StatusCode);
    }
}
