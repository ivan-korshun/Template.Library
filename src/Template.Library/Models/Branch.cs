// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.Models;

    /// <summary>TBD</summary>
public sealed class Branch
{
    /// <summary>TBD</summary>
    public required int Id { get; set; }

    /// <summary>TBD</summary>
    public required Country Country { get; set; }

    /// <summary>TBD</summary>
    public required string City { get; set; }

    /// <summary>TBD</summary>
    public required string Street { get; set; }

    /// <summary>TBD</summary>
    public required int BuildingNumber { get; set; }

    /// <summary>TBD</summary>
    public required DateTime AddedAt { get; set; }

    /// <summary>TBD</summary>
    public string? Phone { get; set; }
}
