// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel.DataAnnotations;

namespace Template.Library.WebApi.Models;

public class BookCheckpointQuery : CheckpointPaging
{
    /// <summary>
    /// TBD
    /// </summary>
    [Length(1, 255)]
    public string? Title { get; init; }
}
