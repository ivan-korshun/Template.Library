// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using System.Net.Http.Headers;

namespace Template.Library.Common.Http.ClientCredentials;

public class AccessTokenHandler(IAccessTokenManager tokenTokenManager) : DelegatingHandler
{
    private readonly IAccessTokenManager _tokenManager = tokenTokenManager;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string accessToken = await _tokenManager.GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
