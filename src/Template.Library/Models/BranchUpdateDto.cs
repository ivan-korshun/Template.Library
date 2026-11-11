// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.Models;

/// <summary>TBD</summary>
public sealed class BranchUpdateDto
{
    /// <summary>TBD</summary>
    [EnumDataType(typeof(Country))]
    public Country? Country { get; init; }

    /// <summary>TBD</summary>
    [MinLength(2)]
    [MaxLength(255)]
    public string? City { get; init; }

    /// <summary>TBD</summary>
    [MinLength(2)]
    [MaxLength(255)]
    public string? Street { get; init; }

    /// <summary>TBD</summary>
    [Range(1, 1000)]
    public int? BuildingNumber { get; init; }

    /// <summary>TBD</summary>
    [Phone]
    public string? Phone { get; init; }
}
