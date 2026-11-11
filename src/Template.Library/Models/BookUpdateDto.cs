// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using Template.Library.Common.Annotations;

namespace Template.Library.Models;

/// <summary>TBD</summary>
public sealed class BookUpdateDto
{
    /// <summary>TBD</summary>
    [MinLength(1)]
    [MaxLength(255)]
    public string? Title { get; init; }

    /// <summary>TBD</summary>
    [EnumDataType(typeof(Genre))]
    public Genre? Genre { get; init; }

    /// <summary>TBD</summary>
    [Range(1, 32000)]
    public int? PageCount { get; init; }

    /// <summary>TBD</summary>
    public bool? Available { get; init; }

    /// <summary>TBD</summary>
    public DateTime? AddedAt { get; set; }

    /// <summary>TBD</summary>
    [MaxLength(20)]
    [ElementNotNullOrEmptyString]
    [ElementMaxStringLength(255)]
    public ICollection<string>? Authors { get; init; }
}
