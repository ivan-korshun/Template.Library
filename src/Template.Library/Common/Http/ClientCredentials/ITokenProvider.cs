// Copyright © Korshun Ivan. All rights reserved.

using Template.Library.Common.Http.ClientCredentials.Models;

namespace Template.Library.Common.Http.ClientCredentials;

public interface ITokenProvider
{
    Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken);
}
