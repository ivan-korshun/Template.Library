// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Template.Library.Models;

/// <summary>TBD</summary>
public sealed class BranchCreateDto(
    // constructor parameters make missing required reference fields reported correctly without making them nullable
    [DisallowNull] Country? country, string city, string street, int buildingNumber, [DisallowNull] DateTime? addedAt)
{
    /// <summary>TBD</summary>
    [NotNull]
    [Required]
    [EnumDataType(typeof(Country))]
    public Country? Country { get; init; } = country; // constructor trick does not work for value types

    /// <summary>TBD</summary>
    [Required]
    [MaxLength(255)]
    public string City { get; init; } = city;

    /// <summary>TBD</summary>
    [Required]
    [MaxLength(255)]
    public string Street { get; init; } = street;

    /// <summary>TBD</summary>
    [Required]
    [Range(1, 1000)]
    public int BuildingNumber { get; init; } = buildingNumber;

    /// <summary>TBD</summary>
    [NotNull]
    [Required]
    public DateTime? AddedAt { get; init; } = addedAt; // constructor trick does not work for value types

    /// <summary>TBD</summary>
    [Phone]
    public string? Phone { get; init; }
}
