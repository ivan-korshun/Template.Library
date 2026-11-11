// Copyright © Korshun Ivan. All rights reserved.

using AutoFixture;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Template.Library.Common.Http.ClientCredentials;
using Template.Library.Common.Http.ClientCredentials.Models;

namespace Template.Library.UnitTests.Common.Http.ClientCredentials;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "SUT is disposed in TearDown")]
public class AccessTokenManagerTests
{
    private ITokenProvider _tokenProvider = null!;
    private TokenRequest _tokenRequest = null!;
    private AccessTokenManager _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _tokenProvider = Substitute.For<ITokenProvider>();

        var fixture = new Fixture();
        _tokenRequest = new TokenRequest
        {
            TokenEndpoint = fixture.Create<string>(),
            ClientId = fixture.Create<string>(),
            ClientSecret = fixture.Create<string>(),
            Audience = fixture.Create<string>(),
        };

        _sut = new AccessTokenManager(_tokenProvider, _tokenRequest);
    }

    [TearDown]
    public void TearDown()
    {
        _sut.Dispose();
    }

    [Test]
    public async Task GetTokenAsync_Always_CashesTokenObtainedFromAuth0()
    {
        // Arrange
        var cancellationToken = new CancellationToken(false);

        _tokenProvider.GetTokenAsync(Arg.Any<TokenRequest>(), cancellationToken)
            .Returns(
                new TokenResponse
                {
                    AccessToken = "token1",
                    ExpiresIn = 3600
                },
                new TokenResponse
                {
                    AccessToken = "token2",
                    ExpiresIn = 3600
                })
            .AndDoes(x => x.Arg<TokenRequest>()
                .Should().BeEquivalentTo(_tokenRequest, config => config.ExcludingMissingMembers()));

        // Act
        string resultA = await _sut.GetTokenAsync(TimeSpan.FromMinutes(1), cancellationToken);
        string resultB = await _sut.GetTokenAsync(TimeSpan.FromMinutes(1), cancellationToken);

        // Assert
        resultA.Should().Be("token1");
        resultB.Should().Be("token1");

        await _tokenProvider.ReceivedWithAnyArgs(1)
            .GetTokenAsync(Arg.Any<TokenRequest>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetTokenAsync_NoExpirationBuffer_ThreeMinuteExpirationBuffer()
    {
        // Arrange
        var cancellationToken = new CancellationToken(false);

        _tokenProvider.GetTokenAsync(Arg.Any<TokenRequest>(), cancellationToken).Returns(
            new TokenResponse
            {
                AccessToken = "token1",
                ExpiresIn = 179 // expires sooner than the default 3-minute (180s) buffer
            },
            new TokenResponse
            {
                AccessToken = "token2",
                ExpiresIn = 200
            });

        await _sut.GetTokenAsync(cancellationToken);

        // Act
        string result = await _sut.GetTokenAsync(cancellationToken);

        // Assert
        result.Should().Be("token2");
    }

    [Test]
    public async Task GetTokenAsync_NullAccessTokenReturned_InvalidOperationException()
    {
        // Arrange
        var cancellationToken = new CancellationToken(false);

        _tokenProvider.GetTokenAsync(Arg.Any<TokenRequest>(), cancellationToken).Returns(new TokenResponse
        {
            AccessToken = null,
            ExpiresIn = 3600
        });

        // Act
        Func<Task> act = () => _sut.GetTokenAsync(TimeSpan.FromMinutes(3), cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task GetTokenAsync_ExpiredTokenInCache_ObtainsNewTokenFromAuth0()
    {
        // Arrange
        var cancellationToken = new CancellationToken(false);

        _tokenProvider.GetTokenAsync(Arg.Any<TokenRequest>(), cancellationToken)
            .Returns(
                new TokenResponse
                {
                    AccessToken = "token1",
                    ExpiresIn = -1
                },
                new TokenResponse
                {
                    AccessToken = "token2",
                    ExpiresIn = 3600
                })
            .AndDoes(x => x.Arg<TokenRequest>()
                .Should().BeEquivalentTo(_tokenRequest, config => config.ExcludingMissingMembers()));

        await _sut.GetTokenAsync(TimeSpan.FromMinutes(3), cancellationToken);

        // Act
        string result = await _sut.GetTokenAsync(TimeSpan.FromMinutes(3), cancellationToken);

        // Assert
        result.Should().Be("token2");
    }

    [Test]
    public async Task GetTokenAsync_TwoParallelThreadsAndExpiredToken_TokenIsObtainedOnlyOnceFromAuth0()
    {
        // Arrange
        var cancellationToken = new CancellationToken(false);

        _tokenProvider.GetTokenAsync(Arg.Any<TokenRequest>(), cancellationToken)
            .Returns(
                _ => new TokenResponse
                {
                    AccessToken = "token1",
                    ExpiresIn = -1
                },
                _ =>
                {
                    Thread.Sleep(200);
                    return new TokenResponse
                    {
                        AccessToken = "token2",
                        ExpiresIn = 3600
                    };
                },
                _ => new TokenResponse
                {
                    AccessToken = "token3",
                    ExpiresIn = 3600
                })
            .AndDoes(x => x.Arg<TokenRequest>()
                .Should().BeEquivalentTo(_tokenRequest, config => config.ExcludingMissingMembers()));

        await _sut.GetTokenAsync(TimeSpan.FromMinutes(1), cancellationToken);

        _tokenProvider.ClearReceivedCalls();

        // Act
        var taskA = Task.Run(() => _sut.GetTokenAsync(TimeSpan.FromMinutes(3), cancellationToken));
        var taskB = Task.Run(() => _sut.GetTokenAsync(TimeSpan.FromMinutes(3), cancellationToken));

        Task.WaitAll(taskA, taskB);

        // Assert
        taskA.Result.Should().Be("token2");
        taskB.Result.Should().Be("token2");

        await _tokenProvider.ReceivedWithAnyArgs(1)
            .GetTokenAsync(Arg.Any<TokenRequest>(), Arg.Any<CancellationToken>());
    }
}
