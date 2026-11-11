// Copyright © Korshun Ivan. All rights reserved.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Template.Library.WebApi.Models;

public class OffsetPaging
{
    /// <summary>
    /// The current page number.
    /// </summary>
    [Range(1, 20_000_000)] // Start from 1 to require it easily when needed, as default 0 value is caught by [Range].
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// The actual number of items on a page.
    /// </summary>
    [Required] // to avoid ambiguous exception between offset and checkpoint actions
    [Range(1, 100)]
    public int PageSize { get; init; }

    /// <summary>
    /// Specifies whether to provide total number of items in response.
    /// </summary>
    [JsonIgnore]
    [DefaultValue(false)]
    public bool IncludeTotal { get; set; }
}
