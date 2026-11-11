// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using System.Net.Http.Json;
using Template.Library.Common.Http.ClientCredentials.Models;

namespace Template.Library.Common.Http.ClientCredentials;

public sealed class TokenProvider(IHttpClientFactory clientFactory) : ITokenProvider
{
    private readonly HttpClient _httpClient = clientFactory.CreateClient(HttpClientNames.DefaultStaticClient);

    public async Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = request.ClientId,
            ["client_secret"] = request.ClientSecret
        };

        if (request.Audience != null)
        {
            parameters["audience"] = request.Audience;
        }

        using var content = new FormUrlEncodedContent(parameters);

        using HttpResponseMessage responseMessage = await _httpClient.PostAsync(
            request.TokenEndpoint, content, cancellationToken);

        await responseMessage.EnsureSuccessAsync();

        TokenResponse? tokenResponse = await responseMessage.Content.ReadFromJsonAsync(
            CredentialsJsonContext.Default.TokenResponse, cancellationToken);

        return tokenResponse.EnsureNotNull();
    }
}
