// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Template.Library.Common.Annotations;

namespace Template.Library.Models;

/// <summary>TBD</summary>
public sealed class BookCreateDto(
    // constructor parameters make missing required reference fields reported correctly without making them nullable
    string title, ICollection<string> authors, [DisallowNull] Genre? genre, int pageCount)
{
    /// <summary>TBD</summary>
    [Required]
    [MaxLength(255)]
    public string Title { get; init; } = title;

    /// <summary>TBD</summary>
    [NotNull]
    [Required]
    [EnumDataType(typeof(Genre))]
    public Genre? Genre { get; init; } = genre; // constructor trick does not work for value types

    /// <summary>TBD</summary>
    [Required]
    [Range(1, 32000)]
    public int PageCount { get; init; } = pageCount;

    /// <summary>TBD</summary>
    public bool Available { get; init; }

    /// <summary>TBD</summary>
    public DateTime? AddedAt { get; set; }

    /// <summary>TBD</summary>
    [Required]
    [MaxLength(20)]
    [ElementNotNullOrEmptyString]
    [ElementMaxStringLength(255)]
    public ICollection<string> Authors { get; init; } = authors;
}
