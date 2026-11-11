// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Template.Library.Common.Options;

namespace Template.Library.WebApi.HealthCheck;

public class HealthChecksOptions : IOptionsBase
{
    static string IOptionsBase.Key => "HealthChecks";

    [Required]
    public required string HealthEndpoint { get; set; }

    [Required]
    public required string LivenessEndpoint { get; set; }

    [Required]
    public required string ReadinessEndpoint { get; set; }

    [Required]
    public required string StartedEndpoint { get; set; }

    [Required]
    public required TimeSpan CachedResultExpiration { get; set; }

    [Required]
    public required string[] Tags { get; set; }
}
