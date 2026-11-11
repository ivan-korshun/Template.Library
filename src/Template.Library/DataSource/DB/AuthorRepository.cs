// Copyright © Korshun Ivan. All rights reserved.

using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Template.Library.Common.Exceptions;
using Template.Library.Common.Redaction;
using Template.Library.DataSource.Abstraction;
using Template.Library.DataSource.Dapper;
using Template.Library.DataSource.DB.Records;
using Template.Library.Models;

namespace Template.Library.DataSource.DB;

public sealed partial class AuthorRepository(
    IDbDataProviderFactory factory, ILogger<AuthorRepository> logger, IOptions<DatabaseOptions> options)
    : IAuthorRepository
{
    private readonly IDbDataProviderFactory _factory = factory;
    private readonly ILogger<AuthorRepository> _logger = logger;
    private readonly string _connectionString = options.Value.ConnectionString;

    public Task<DataFragment<Author>> GetAuthors(int? from, int limit, CancellationToken token = default)
    {
        return GetAuthors(from, limit, null, token);
    }

    public async Task<DataFragment<Author>> GetAuthors(
        int? from, int limit, string? fullName, CancellationToken token = default)
    {
        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        IEnumerable<AuthorRecord> authors = await dataProvider.GetPageAsync<AuthorRecord>(
            TableNames.Authors,
            limit,
            fromColumn: "SerialNumber",
            from,
            !string.IsNullOrEmpty(fullName) ? [new(nameof(Author.FullName), SqlOperator.Contains, fullName)] : null,
            token: token);

        return new DataFragment<Author>(
            [.. authors.Select(b => b.ToAuthor())],
            authors.LastOrDefault()?.SerialNumber.ToString(CultureInfo.InvariantCulture));
    }

    public async Task<DataFragment<Author>> GetAuthors(
        int pageNumber, int pageSize, bool includeTotal = false, CancellationToken token = default)
    {
        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        PagedResult<AuthorRecord> result = await dataProvider.GetOffsetPageAsync<AuthorRecord>(
            TableNames.Authors,
            pageNumber,
            pageSize,
            orderBy: nameof(AuthorRecord.FullName),
            includeTotal: includeTotal,
            token: token);

        return new DataFragment<Author>(
            [.. result.Page.Select(b => b.ToAuthor())],
            Total: result.Total);
    }

    public async Task<Author?> GetAuthor(Guid id, CancellationToken token = default)
    {
        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        AuthorRecord? author = await dataProvider.GetByIdAsync<AuthorRecord>(TableNames.Authors, "Id", id, token);
        return author?.ToAuthor();
    }

    public async Task<Author> CreateAuthor(AuthorCreateDto author, CancellationToken token = default)
    {
        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        try
        {
            AuthorRecord authorRecord = await dataProvider.InsertAsync<AuthorRecord>(
                TableNames.Authors,
                [
                    new(nameof(BookRecord.Id), Guid.NewGuid()),
                    new(nameof(AuthorRecord.FullName), author.FullName),
                    new(nameof(AuthorRecord.Country), author.Country?.ToString()),
                    new(nameof(AuthorRecord.Birthday), author.Birthday),
                    new(nameof(AuthorRecord.Famous), author.Famous)
                ],
                token: token);

            return authorRecord.ToAuthor();
        }
        catch (UniqueConstraintException e)
        {
            LogDuplicateConflictInformation(e.DuplicateValue.MaskProportional());
            throw new ConflictException(null, $"Cannot set duplicate value '{e.DuplicateValue}'.", e);
        }
    }

    public async Task<Author?> UpdateAuthor(
        Guid id, AuthorUpdateDto author, string? eTag, CancellationToken token = default)
    {
        List<SqlCondition> where = [new(nameof(AuthorRecord.Id), id)];
        if (!string.IsNullOrEmpty(eTag))
        {
            where.Add(new(nameof(AuthorRecord.RowVersion), OptimisticRecord.ParseVersionString(eTag)));
        }

        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        try
        {
            AuthorRecord? authorRecord = await dataProvider.UpdateIfNotNullAsync<AuthorRecord>(
                TableNames.Authors,
                [
                    new(nameof(AuthorRecord.FullName), author.FullName),
                    new(nameof(AuthorRecord.Country), author.Country?.ToString()),
                    new(nameof(AuthorRecord.Birthday), author.Birthday),
                    new(nameof(AuthorRecord.Famous), author.Famous)
                ],
                where,
                token: token);

            if (authorRecord == null && !string.IsNullOrEmpty(eTag))
            {
                throw new ETagMismatchException();
            }

            return authorRecord?.ToAuthor();
        }
        catch (UniqueConstraintException e)
        {
            LogDuplicateConflictInformation(e.DuplicateValue.MaskProportional());
            throw new ConflictException(null, $"Cannot set duplicate value '{e.DuplicateValue}'.", e);
        }
    }

    public async Task<bool> DeleteAuthor(Guid id, CancellationToken token = default)
    {
        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        int rowCount = await dataProvider.DeleteAsync(
            TableNames.Authors, [new(nameof(AuthorRecord.Id), id)], token: token);

        return rowCount > 0;
    }

    [LoggerMessage(LogLevel.Information, "Cannot set duplicate value '{Value}'.")]
    private partial void LogDuplicateConflictInformation(string? value);
}
