// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.DataSource.Dapper;

public class PagedResult<T>
{
    public required IEnumerable<T> Page { get; init; }

    public int? Total { get; init; }
}

