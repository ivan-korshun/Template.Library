// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.Common.Http.ClientCredentials.Models;

public sealed class TokenRequest
{
    [Required]
    public required string TokenEndpoint { get; set; }

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }

    public string? Audience { get; set; }
}
