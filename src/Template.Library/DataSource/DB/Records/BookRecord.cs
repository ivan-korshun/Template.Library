// Copyright © Korshun Ivan. All rights reserved.

using Template.Library.Models;

namespace Template.Library.DataSource.DB.Records;

public class BookRecord : OptimisticRecord
{
    public required int SerialNumber { get; init; }

    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required int PageCount { get; init; }

    public required Genre Genre { get; init; }

    public bool Available { get; init; }

    public DateTime? AddedAt { get; set; }

    public string? Authors { get; init; }

    public Book ToBook()
    {
        return new Book
        {
            Id = Id,
            Title = Title,
            PageCount = PageCount,
            Genre = Genre,
            Available = Available,
            AddedAt = AddedAt,
            Authors = Authors?.Split(',') ?? [],
            Version = GetVersionString(RowVersion)
        };
    }
}
