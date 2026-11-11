// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using Template.Library.Models;

namespace Template.Library.WebApi.Models;

/// <summary>
/// The page of items as well as checkpoint pagination related metadata.
/// </summary>
public class CheckpointPage<T> : CheckpointPaging
{
    [SetsRequiredMembers]
    public CheckpointPage(DataFragment<T> dataFragment, CheckpointPaging paging)
    {
        From = paging.From;
        Limit = paging.Limit;
        Data = dataFragment.Data;
        Next = dataFragment.Next;
    }

    /// <summary>
    /// The collection of items in the page.
    /// </summary>
    public ICollection<T> Data { get; init; }

    /// <summary>
    /// Checkpoint to obtain the next page. Null value indicates the last page.
    /// </summary>
    public string? Next { get; init; }
}
