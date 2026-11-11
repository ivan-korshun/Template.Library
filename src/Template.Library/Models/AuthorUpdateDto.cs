// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.Models;

/// <summary>TBD</summary>
public sealed class AuthorUpdateDto
{
    /// <summary>TBD</summary>
    [MaxLength(255)]
    public string? FullName { get; init; }

    /// <summary>TBD</summary>
    [EnumDataType(typeof(Country))]
    public Country? Country { get; init; }

    /// <summary>TBD</summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>TBD</summary>
    public bool? Famous { get; init; }
}
