// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.WebApi.Models;

/// <summary>TBD</summary>
public class Page<T>(ICollection<T> data)
{
    /// <summary>TBD</summary>
    public ICollection<T> Data { get; } = data;
}
