// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.Models;

/// <summary>
/// The page of items.
/// </summary>
/// <param name="Data">The collection of items in the page.</param>
/// <param name="Next">Checkpoint to obtain the next page (if applicable).</param>
/// <param name="Total">The total number of items available (if applicable).</param>
public sealed record DataFragment<T>(ICollection<T> Data, string? Next = null, int? Total = null);
