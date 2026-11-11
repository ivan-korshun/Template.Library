// Copyright © Korshun Ivan. All rights reserved.

using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using AutoFixture.NUnit4;
using FluentAssertions;
using NSubstitute;
using NSubstitute.DbConnection;
using NUnit.Framework;
using Polly;
using Template.Library.DataSource.Dapper;

namespace Template.Library.UnitTests.DataSource.Dapper;

[TestFixture]
[SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "_sut dependencies are mocked")]
public sealed class DbDataProviderTests
{
    private DbConnection _connection = null!;
    private DbDataProvider _sut = null!;

    private int _idempotentCallsCount;
    private int _nonIdempotentCallsCount;

    [SetUp]
    public void SetUp()
    {
        _connection = Substitute.For<DbConnection>().SetupCommands();

        _idempotentCallsCount = 0;
        var idempotentRetry = new ResiliencePipelineBuilder()
            .AddStrategy(_ => new TestTrackingStrategy(() => _idempotentCallsCount++))
            .Build();

        _nonIdempotentCallsCount = 0;
        var nonIdempotentRetry = new ResiliencePipelineBuilder()
            .AddStrategy(_ => new TestTrackingStrategy(() => _nonIdempotentCallsCount++))
            .Build();

        _sut = new DbDataProvider(_connection, idempotentRetry, nonIdempotentRetry);
    }

    [Test, AutoData]
    public async Task GetByIdAsync_Found_Record(string table, string column, object value, TestRecord record)
    {
        // Arrange
        _connection.SetupQuery($"SELECT * FROM [{table}] WHERE [{column}] = @{column}")
            .WithParameter(column, value)
            .Returns([record]);

        // Act
        TestRecord? result = await _sut.GetByIdAsync<TestRecord>(table, column, value);

        // Assert
        result.Should().BeEquivalentTo(record);
        _idempotentCallsCount.Should().Be(1);
        _nonIdempotentCallsCount.Should().Be(0);
    }

    [Test, AutoData]
    public async Task GetByIdAsync_NotFound_Null(string table, string column, object value)
    {
        // Arrange
        _connection.SetupQuery($"SELECT * FROM [{table}] WHERE [{column}] = @{column}")
            .Returns(Array.Empty<TestRecord>());

        // Act
        TestRecord? result = await _sut.GetByIdAsync<TestRecord>(table, column, value);

        // Assert
        result.Should().BeNull();
        _idempotentCallsCount.Should().Be(1);
        _nonIdempotentCallsCount.Should().Be(0);
    }

    // TODO: complete unit tests

    // https://github.com/jmg48/NSubstitute.DbConnection/blob/main/NSubstitute.DbConnection/NSubstitute.DbConnection.Tests/NonQueryTests.cs
    /* mockConnection.SetupQuery("select * from table").Affects(3); */

    // https://github.com/jmg48/NSubstitute.DbConnection/blob/main/NSubstitute.DbConnection/NSubstitute.DbConnection/IMockQueryBuilder.cs

    //IEnumerable<BookRecord> books = await connection.GetPageAsync<BookRecord>(
    //    TableNames.Books,
    //    limit,
    //    fromColumn: "SerialNumber",
    //    from,
    //    where: [
    //        new("Version", 1),
    //        new(nameof(Book.Genre), SqlOperator.NotEqual, Genre.Science.ToString()),
    //        new(nameof(Book.Title), SqlOperator.IsNotNull)],
    //    token: token);

    //IEnumerable<BookRecord> books = await connection.GetPageAsync<BookRecord>(
    //    TableNames.Books,
    //    limit,
    //    fromColumn: "SerialNumber",
    //    from,
    //    whereSql: $"{nameof(Book.Genre)} = @genre",
    //    whereParams: [new("genre", Genre.Fantasy.ToString())],
    //    token: token);

    //IEnumerable<BookRecord> books = await connection.GetPageAsync<BookRecord>(
    //    TableNames.Books,
    //    limit,
    //    fromColumn: "SerialNumber",
    //    from,
    //    b => b.Where($"{nameof(Book.Genre)} = @genre", new { genre = Genre.Fantasy.ToString() }),
    //    token);



    //PagedResult<BookRecord> result = await connection.GetOffsetPageAsync<BookRecord>(
    //    TableNames.Books,
    //    pageNumber,
    //    pageSize,
    //    orderBy: nameof(BookRecord.Title),
    //    where: [
    //        new(nameof(BookRecord.PageCount), 11),
    //        new(nameof(BookRecord.Genre), SqlOperator.NotEqual, Genre.Science.ToString()),
    //        new(nameof(BookRecord.Title), SqlOperator.IsNotNull),
    //        new(nameof(BookRecord.Title), SqlOperator.Contains, "a")],
    //    includeTotal: includeTotal,
    //    token: token);

    //IEnumerable<BookRecord> books = await connection.GetOffsetPageAsync<BookRecord>(
    //    TableNames.Books,
    //    pageNumber,
    //    pageSize,
    //    orderBy: nameof(Book.Title),
    //    where: [new("Version", 1), new(nameof(Book.Genre), Genre.Comedy.ToString())],
    //    token);

    //IEnumerable<BookRecord> books = await connection.GetOffsetPageAsync<BookRecord>(
    //    TableNames.Books,
    //    pageNumber,
    //    pageSize,
    //    orderBy: nameof(Book.Title),
    //    whereSql: $"[Version] = @Version OR [{nameof(Book.Genre)}] = @{nameof(Book.Genre)}",
    //    [new("Version", 3), new(nameof(Book.Genre), Genre.Comedy.ToString())],
    //    token);

    //PagedResult<BookRecord> result = await connection.GetOffsetPageAsync<BookRecord>(
    //    TableNames.Books,
    //    pageNumber,
    //    pageSize,
    //    orderBy: nameof(Book.Title),
    //    b => b.Where(
    //        $"[PageCount] = @PageCount OR [{nameof(Book.Genre)}] = @{nameof(Book.Genre)}",
    //        new { PageCount = 11, Genre = Genre.Comedy.ToString() }),
    //    includeTotal: true,
    //    token: token);

    public record TestRecord(Guid Id, string Text, bool Flag, byte[] RowVersion);

    private class TestTrackingStrategy(Action onExecute) : ResilienceStrategy
    {
        private readonly Action _onExecute = onExecute;

        protected override ValueTask<Outcome<TResult>> ExecuteCore<TResult, TState>(
            Func<ResilienceContext, TState, ValueTask<Outcome<TResult>>> callback,
            ResilienceContext context,
            TState state)
        {
            _onExecute();
            return callback(context, state);
        }
    }
}
