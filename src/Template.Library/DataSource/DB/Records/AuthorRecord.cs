// Copyright © Korshun Ivan. All rights reserved.

using Template.Library.Models;

namespace Template.Library.DataSource.DB.Records;

public class AuthorRecord : OptimisticRecord
{
    public required int SerialNumber { get; init; }

    public required Guid Id { get; init; }

    public required string FullName { get; init; }

    public Country? Country { get; init; }

    public DateOnly? Birthday { get; set; }

    public bool Famous { get; init; }

    public Author ToAuthor()
    {
        return new Author
        {
            Id = Id,
            FullName = FullName,
            Country = Country,
            Birthday = Birthday,
            Famous = Famous,
            Version = GetVersionString(RowVersion)
        };
    }
}
