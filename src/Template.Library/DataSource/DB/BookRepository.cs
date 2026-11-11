// Copyright © Korshun Ivan. All rights reserved.

using System.Data;
using System.Globalization;
using Dapper;
using Microsoft.Extensions.Options;
using Template.Library.Common.Exceptions;
using Template.Library.DataSource.Abstraction;
using Template.Library.DataSource.Dapper;
using Template.Library.DataSource.DB.Records;
using Template.Library.Models;

namespace Template.Library.DataSource.DB;

public sealed class BookRepository(IDbDataProviderFactory factory, IOptions<DatabaseOptions> options) : IBookRepository
{
    private readonly IDbDataProviderFactory _factory = factory;
    private readonly string _connectionString = options.Value.ConnectionString;

    public Task<DataFragment<Book>> GetBooks(int? from, int limit, CancellationToken token = default)
    {
        return GetBooks(from, limit, null, token);
    }

    public async Task<DataFragment<Book>> GetBooks(int? from, int limit, string? title, CancellationToken token = default)
    {
        var builder = new SqlBuilder();
        var template = builder.AddTemplate(Sql.BookCheckpointPageTemplate, new { limit });

        if (from.HasValue)
        {
            builder.Where("SerialNumber > @from", new { from });
        }

        if (!string.IsNullOrEmpty(title))
        {
            var condition = new SqlCondition(nameof(Book.Title), SqlOperator.Contains, title);
            builder.Where($"{condition.Column} like @title", new { title = condition.Value });
        }

        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        IEnumerable<BookRecord> books = await dataProvider.QueryAsync<BookRecord>(
            template.RawSql, template.Parameters, token);

        return new DataFragment<Book>(
            [.. books.Select(b => b.ToBook())],
            books.LastOrDefault()?.SerialNumber.ToString(CultureInfo.InvariantCulture));
    }

    public async Task<DataFragment<Book>> GetBooks(
        int pageNumber, int pageSize, bool includeTotal = false, CancellationToken token = default)
    {
        var parameters = new { pageNumber, pageSize };

        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        if (includeTotal)
        {
            using SqlMapper.GridReader multi = await dataProvider.QueryMultipleAsync(
                $"{Sql.BookCount}; {Sql.BookOffsetPage}", parameters, token);

            int total = await multi.ReadFirstAsync<int>();
            IEnumerable<BookRecord> items = await multi.ReadAsync<BookRecord>();
            return new DataFragment<Book>([.. items.Select(b => b.ToBook())], Total: total);
        }
        else
        {
            IEnumerable<BookRecord> books = await dataProvider.QueryAsync<BookRecord>(
                Sql.BookOffsetPage, parameters, token);

            return new DataFragment<Book>([.. books.Select(b => b.ToBook())]);
        }
    }

    public async Task<Book?> GetBook(Guid id, CancellationToken token = default)
    {
        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        BookRecord? book = await dataProvider.QuerySingleOrDefaultAsync<BookRecord>(Sql.BookById, new { id }, token);
        return book?.ToBook();
    }

    /// <summary>
    /// Artificial use case to illustrate complex insert into 3 related tables with transaction.
    /// </summary>
    public async Task<Book> CreateBook(BookCreateDto book, CancellationToken token = default)
    {
        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        BookRecord bookRecord = await dataProvider.AsTransactionAsync(async db =>
        {
            var createdRecord = await db.InsertAsync<BookRecord>(
                TableNames.Books,
                [
                    new(nameof(BookRecord.Id), Guid.NewGuid()),
                    new(nameof(BookRecord.Title), book.Title),
                    new(nameof(BookRecord.PageCount), book.PageCount),
                    new(nameof(BookRecord.Genre), book.Genre.ToString()),
                    new(nameof(BookRecord.Available), book.Available),
                    new(nameof(BookRecord.AddedAt), book.AddedAt)
                ],
                 token);

            if (book.Authors.Count > 0)
            {
                await db.InsertIfNotExistsAsync(
                    TableNames.Authors,
                    columns: [nameof(AuthorRecord.Id), nameof(AuthorRecord.FullName)],
                    multiValues: book.Authors.Select(author => (IList<object?>)[Guid.NewGuid(), author]).ToList(),
                    matchingColumns: [nameof(AuthorRecord.FullName)],
                    token);

                IEnumerable<int> authorNumbers = await db.GetColumnAsync<int>(
                    TableNames.Authors,
                    nameof(AuthorRecord.SerialNumber),
                    [new(nameof(AuthorRecord.FullName), SqlOperator.In, book.Authors)],
                    token);

                await db.InsertAsync(
                    TableNames.BookAuthors,
                    columns: [nameof(BookAuthorRecord.BookSn), nameof(BookAuthorRecord.AuthorSn)],
                    multiValues: authorNumbers.Select(an => (IList<object?>)[createdRecord.SerialNumber, an]).ToList(),
                    token: token);
            }

            return createdRecord;
        }, token);

        var result = bookRecord.ToBook();
        result.Authors = book.Authors;
        return result;
    }

    /// <summary>
    /// Artificial use case to illustrate complex update of 3 related tables with transaction.
    /// </summary>
    /// <exception cref="ETagMismatchException"></exception>
    public async Task<Book?> UpdateBook(Guid id, BookUpdateDto book, string? eTag, CancellationToken token = default)
    {
        List<SqlCondition> where = [new(nameof(BookRecord.Id), id)];
        if (!string.IsNullOrEmpty(eTag))
        {
            where.Add(new(nameof(BookRecord.RowVersion), OptimisticRecord.ParseVersionString(eTag)));
        }

        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        BookRecord? bookRecord = await dataProvider.AsTransactionAsync(async db =>
        {
            var updatedRecord = await db.UpdateIfNotNullAsync<BookRecord>(
                TableNames.Books,
                [
                    new(nameof(BookRecord.Title), book.Title),
                    new(nameof(BookRecord.Genre), book.Genre?.ToString()),
                    new(nameof(BookRecord.PageCount), book.PageCount),
                    new(nameof(BookRecord.Available), book.Available),
                    new(nameof(BookRecord.AddedAt), book.AddedAt)
                ],
                where,
                token: token);

            if (updatedRecord != null && book.Authors?.Count > 0)
            {
                await db.InsertIfNotExistsAsync(
                    TableNames.Authors,
                    columns: [nameof(AuthorRecord.Id), nameof(AuthorRecord.FullName)],
                    multiValues: book.Authors.Select(author => (IList<object?>)[Guid.NewGuid(), author]).ToList(),
                    matchingColumns: [nameof(AuthorRecord.FullName)],
                    token: token);

                IEnumerable<int> authorNumbers = await db.GetColumnAsync<int>(
                    TableNames.Authors,
                    nameof(AuthorRecord.SerialNumber),
                    [new(nameof(AuthorRecord.FullName), SqlOperator.In, book.Authors)],
                    token);

                await MergeBookAuthors(db, updatedRecord.SerialNumber, authorNumbers, token);

                // Merge alternative via 2 requests:
                // await InsertDeleteBookAuthors(db, updatedRecord.SerialNumber, authorNumbers, token);
            }

            return updatedRecord;
        }, token);

        if (bookRecord == null)
        {
            return string.IsNullOrEmpty(eTag) ? null : throw new ETagMismatchException();
        }

        // One more request to get all authors for the book
        var result = await dataProvider.QuerySingleOrDefaultAsync<BookRecord>(Sql.BookById, new { bookRecord.Id }, token);
        return result?.ToBook();
    }

    public async Task<bool> DeleteBook(Guid id, CancellationToken token = default)
    {
        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        int rowCount = await dataProvider.DeleteAsync(TableNames.Books, [new(nameof(BookRecord.Id), id)], token);
        return rowCount > 0;
    }

    private static async Task InsertDeleteBookAuthors(
        IDbDataProvider dataProvider, int bookNumber, IEnumerable<int> authorNumbers, CancellationToken token)
    {
        await dataProvider.DeleteAsync(
            TableNames.BookAuthors,
            [
                new(nameof(BookAuthorRecord.BookSn), bookNumber),
                new(nameof(BookAuthorRecord.AuthorSn), SqlOperator.NotIn, authorNumbers)
            ],
            token);

        await dataProvider.InsertIfNotExistsAsync(
            TableNames.BookAuthors,
            columns: [nameof(BookAuthorRecord.BookSn), nameof(BookAuthorRecord.AuthorSn)],
            multiValues: authorNumbers.Select(an => (IList<object?>)[bookNumber, an]).ToList(),
            matchingColumns: [nameof(BookAuthorRecord.BookSn), nameof(BookAuthorRecord.AuthorSn)],
            token);
    }

    private static ValueTask<int> MergeBookAuthors(
        IDbDataProvider dataProvider, int bookNumber, IEnumerable<int> authorNumbers, CancellationToken token)
    {
        List<string> valuesParts = [];
        var parameters = new DynamicParameters();
        parameters.Add("TargetBookSn", bookNumber);

        int i = 0;
        foreach (var authorNumber in authorNumbers)
        {
            valuesParts.Add(string.Concat("(@TargetBookSn, @AuthorNumber", ++i, ")"));
            parameters.Add("AuthorNumber" + i, authorNumber);
        }

        string valuesSql = string.Join(", ", valuesParts);

        return dataProvider.ExecuteAsync(
            $"""
            MERGE {TableNames.BookAuthors} AS Target
            USING (VALUES {valuesSql}) AS Source (BookSn, AuthorSn)
            ON (Target.BookSn = Source.BookSn AND Target.AuthorSn = Source.AuthorSn)

            WHEN NOT MATCHED BY TARGET THEN INSERT (BookSn, AuthorSn) VALUES (Source.BookSn, Source.AuthorSn)

            WHEN NOT MATCHED BY SOURCE AND Target.BookSn = @TargetBookSn THEN DELETE;
            """,
            parameters,
            token);
    }

    private static class Sql
    {
        public const string BookCount = $"SELECT COUNT(*) FROM [{TableNames.Books}]";

        public const string BookById = $"""
            SELECT {AliasedFields}, sub.Authors FROM {TableNames.Books} b
            {OuterApplyAuthors}
            WHERE b.Id = @id
            """;

        public const string BookOffsetPage = $"""
            SELECT {AliasedFields}, sub.Authors
            FROM (
                SELECT {StandardFields} FROM {TableNames.Books} ORDER BY Title
                {SqlConstants.OffsetPaging}
            ) b {OuterApplyAuthors}
            """;

        public const string BookCheckpointPageTemplate = $"""
            SELECT {AliasedFields}, sub.Authors
            FROM (
                SELECT TOP(@limit) {StandardFields} FROM {TableNames.Books} /**where**/ ORDER BY SerialNumber ASC
            ) b {OuterApplyAuthors}
            """;

        private const string OuterApplyAuthors = $"""
            OUTER APPLY (
                SELECT STRING_AGG(a.FullName, ',') AS Authors
                FROM {TableNames.BookAuthors} ba JOIN {TableNames.Authors} a
                ON ba.AuthorSn = a.SerialNumber
                WHERE ba.BookSn = b.SerialNumber
            ) sub
            """;

        private const string StandardFields = "SerialNumber, Id, Title, PageCount, Genre, Available, AddedAt, RowVersion";
        private const string AliasedFields = "b.SerialNumber, b.Id, b.Title, b.PageCount, b.Genre, b.Available, b.AddedAt, b.RowVersion";
    }
}
