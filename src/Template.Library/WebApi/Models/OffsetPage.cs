// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using Template.Library.Models;

namespace Template.Library.WebApi.Models;

/// <summary>
/// The page of items as well as offset pagination related metadata.
/// </summary>
public sealed class OffsetPage<T> : OffsetPaging
{
    [SetsRequiredMembers]
    public OffsetPage(DataFragment<T> dataFragment, OffsetPaging paging)
    {
        Data = dataFragment.Data;
        Total = dataFragment.Total;
        PageNumber = paging.PageNumber;
        PageSize = paging.PageSize;
    }

    /// <summary>
    /// The collection of items in the page.
    /// </summary>
    public ICollection<T> Data { get; }

    /// <summary>
    /// The total number of items available.
    /// </summary>
    public int? Total { get; set; }
}
