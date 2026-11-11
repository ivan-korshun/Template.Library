// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.Common.Http;

public abstract class ServiceOptionsBase
{
    [Url]
    [Required]
    public string Host { get; set; } = string.Empty;

    [Required]
    public string ClientCredentials { get; set; } = string.Empty;
}
