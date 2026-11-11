// Copyright © Korshun Ivan. All rights reserved.

using AutoFixture.NUnit4;
using FluentAssertions;
using NUnit.Framework;
using Template.Library.DataSource.DB.Records;
using Template.Library.Models;

namespace Template.Library.UnitTests.DataSource.DB.Records;

[TestFixture]
public sealed class BookRecordTests
{
    [Test, AutoData]
    public async Task ToBook_Always_Book(BookRecord bookRecord)
    {
        bookRecord.ToBook().Should().BeEquivalentTo(new Book
        {
            Id = bookRecord.Id,
            Title = bookRecord.Title,
            PageCount = bookRecord.PageCount,
            Genre = bookRecord.Genre,
            Available = bookRecord.Available,
            AddedAt = bookRecord.AddedAt,
            Authors = bookRecord.Authors?.Split(',') ?? [],
            Version = Convert.ToBase64String(bookRecord.RowVersion)
        });
    }
}
