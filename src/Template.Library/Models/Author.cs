// Copyright © Korshun Ivan. All rights reserved.

using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Template.Library.Models;

/// <summary>TBD</summary>
public sealed class Author
{
    /// <summary>TBD</summary>
    public required Guid Id { get; set; }

    /// <summary>TBD</summary>
    public required string FullName { get; set; }

    /// <summary>TBD</summary>
    public Country? Country { get; set; }

    /// <summary>TBD</summary>
    public DateOnly? Birthday { get; set; }

    /// <summary>TBD</summary>
    public bool Famous { get; set; }

    [JsonIgnore]
    [XmlIgnore]
    public string? Version { get; set; }
}
