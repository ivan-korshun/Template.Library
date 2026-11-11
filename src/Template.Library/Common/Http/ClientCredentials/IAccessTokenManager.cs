// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.Common.Http.ClientCredentials;

public interface IAccessTokenManager
{
    string TokenEndpoint { get; }

    Task<string> GetTokenAsync(CancellationToken cancellationToken);

    Task<string> GetTokenAsync(TimeSpan expirationBuffer, CancellationToken cancellationToken);
}
