// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.Models;

/// <summary>TBD</summary>
public sealed class AuthorCreateDto(
    // constructor parameters make missing required reference fields reported correctly without making them nullable
    string fullName)
{
    /// <summary>TBD</summary>
    [Required]
    [MaxLength(255)]
    public string FullName { get; init; } = fullName;

    /// <summary>TBD</summary>
    [EnumDataType(typeof(Country))]
    public Country? Country { get; init; }

    /// <summary>TBD</summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>TBD</summary>
    public bool Famous { get; init; }
}
