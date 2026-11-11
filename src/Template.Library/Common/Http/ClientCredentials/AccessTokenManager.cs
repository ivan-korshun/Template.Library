// Copyright © Korshun Ivan. All rights reserved.

using Template.Library.Common.Http.ClientCredentials.Models;
using Template.Library.Common.Threading;

namespace Template.Library.Common.Http.ClientCredentials;

public sealed class AccessTokenManager(ITokenProvider tokenProvider, TokenRequest tokenRequest)
    : IAccessTokenManager, IDisposable
{
    private readonly TokenRequest _request = tokenRequest;
    private readonly ITokenProvider _tokenProvider = tokenProvider;

    private readonly AsyncLock _lock = new();
    private bool _disposed;

    private volatile TokenDetails? _token;

    public string TokenEndpoint => _request.TokenEndpoint;

    public Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        return GetTokenAsync(TimeSpan.FromMinutes(3), cancellationToken);
    }

    public async Task<string> GetTokenAsync(TimeSpan expirationBuffer, CancellationToken cancellationToken)
    {
        if (IsInvalid(_token, expirationBuffer))
        {
            using (await _lock.LockAsync(cancellationToken))
            {
                // check if value was initialized by another thread
                if (IsInvalid(_token, expirationBuffer))
                {
                    _token = await GetTokenCore(cancellationToken);
                }
            }
        }

        return _token!.AccessToken;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _lock.Dispose();
        _disposed = true;
    }

    private async Task<TokenDetails> GetTokenCore(CancellationToken cancellationToken)
    {
        TokenResponse token = await _tokenProvider.GetTokenAsync(_request, cancellationToken);

        return new TokenDetails(token.AccessToken.EnsureNotNull(), DateTime.UtcNow.AddSeconds(token.ExpiresIn));
    }

    private static bool IsInvalid(TokenDetails? token, TimeSpan expirationBuffer)
    {
        return token == null || token.ExpirationTime < DateTime.UtcNow + expirationBuffer;
    }

    private sealed record TokenDetails(string AccessToken, DateTime ExpirationTime);
}
