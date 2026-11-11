// Copyright © Korshun Ivan. All rights reserved.

using System.Text.Json.Serialization;

namespace Template.Library.Common.Http.ClientCredentials.Models;

public sealed class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}
