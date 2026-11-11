// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Template.Library.Models;

/// <summary>TBD</summary>
public sealed class Book
{
    /// <summary>TBD</summary>
    public required Guid Id { get; set; }

    /// <summary>TBD</summary>
    public required string Title { get; set; }

    /// <summary>TBD</summary>
    public required Genre Genre { get; set; }

    /// <summary>TBD</summary>
    public required int PageCount { get; set; }

    /// <summary>TBD</summary>
    public required bool Available { get; set; }

    /// <summary>TBD</summary>
    public DateTime? AddedAt { get; set; }

    /// <summary>TBD</summary>
    public required ICollection<string> Authors { get; set; }

    [JsonIgnore]
    [XmlIgnore]
    public string? Version { get; set; }
}
