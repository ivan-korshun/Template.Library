// Copyright © Korshun Ivan. All rights reserved.

using AutoFixture;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Template.Library.DataSource.Dapper;
using Template.Library.DataSource.DB;
using Template.Library.DataSource.DB.Records;
using Template.Library.Models;

namespace Template.Library.UnitTests.DataSource.DB;

[TestFixture]
public sealed class BookRepositoryTests
{
    private DatabaseOptions _options = null!;
    private IDbDataProvider _dataProvider = null!;
    private BookRepository _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _options = new Fixture().Create<DatabaseOptions>();
        _dataProvider = Substitute.For<IDbDataProvider>();

        var factory = Substitute.For<IDbDataProviderFactory>();
        factory.CreateProvider(_options.ConnectionString).Returns(_dataProvider);

        _sut = new BookRepository(factory, Options.Create(_options));
    }

    [Test, AutoData]
    public async Task GetBook_Always_CallsProvider(BookRecord bookRecord, CancellationToken token)
    {
        // Arrange
        _dataProvider
            .QuerySingleOrDefaultAsync<BookRecord>(
                $"""
                SELECT b.SerialNumber, b.Id, b.Title, b.PageCount, b.Genre, b.Available, b.AddedAt, b.RowVersion, sub.Authors FROM Books b
                OUTER APPLY (
                    SELECT STRING_AGG(a.FullName, ',') AS Authors
                    FROM BookAuthors ba JOIN Authors a
                    ON ba.AuthorSn = a.SerialNumber
                    WHERE ba.BookSn = b.SerialNumber
                ) sub
                WHERE b.Id = @id
                """,
                Arg.Is<object>(v => v.Should().BeEquivalentTo(new { id = bookRecord.Id }) != null),
                token)
            .Returns(bookRecord);

        // Act
        Book? result = await _sut.GetBook(bookRecord.Id, token);

        // Asssert
        result.Should().BeEquivalentTo(bookRecord.ToBook());
    }
}
