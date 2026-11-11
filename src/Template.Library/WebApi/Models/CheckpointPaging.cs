// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.WebApi.Models;

public class CheckpointPaging
{
    /// <summary>
    /// Optional checkpoint to select data from.
    /// </summary>
    [Length(1, 255)]
    public string? From { get; init; }

    /// <summary>
    /// Number of items to select.
    /// </summary>
    [Required] // to avoid ambiguous exception between offset and checkpoint actions
    [Range(1, 100)]
    public int Limit { get; init; }
}
