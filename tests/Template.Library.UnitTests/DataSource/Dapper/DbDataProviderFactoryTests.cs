// Copyright © Korshun Ivan. All rights reserved.

using System.Data.Common;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.DbConnection;
using NUnit.Framework;
using Polly;
using Polly.Retry;
using Template.Library.DataSource.Dapper;

namespace Template.Library.UnitTests.DataSource.Dapper;

[TestFixture]
public sealed class DbDataProviderFactoryTests
{
    private const string ConnectionString = "expected";

    private DbConnection _connection = null!;
    private RetryStrategyOptions _options = null!;
    private FakeLogger<DbDataProviderFactory> _logger = null!;
    private DbDataProviderFactory _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _connection = Substitute.For<DbConnection>().SetupCommands();
        _options = new RetryStrategyOptions { BackoffType = DelayBackoffType.Constant, Delay = TimeSpan.Zero };

        _logger = new FakeLogger<DbDataProviderFactory>();
        var loggerFactory = Substitute.For<ILoggerFactory>();
        loggerFactory.CreateLogger(Arg.Any<string>()).Returns(_logger);

        _sut = new CustomConnectionDbDataProviderFactory(_connection, Options.Create(_options), loggerFactory);
    }

    [Test, AutoData]
    public async Task CreateProvider_IdempotentPolicyOnTransientErrorsIncludingTimeoutCode_Retry(
        string table, string column, object value, TestRecord testRecord)
    {
        // Arrange
        List<Exception> exceptions =
        [
            new TimeoutException(),
            SqlExceptionFactory.CreateSqlException(1205),
            SqlExceptionFactory.CreateSqlException(-2)
        ];

        int callCount = 0;
        _connection.SetupQuery(q => true)
            .Returns<TestRecord>(_ => exceptions.Count > callCount ? throw exceptions[callCount++] : [testRecord]);

        // Act
        var provider = _sut.CreateProvider(ConnectionString);

        // Assert
        var result = await provider.GetByIdAsync<TestRecord>(table, column, value);
        result.Should().Be(testRecord);

        _logger.Messages.Where(m => m.logLevel == LogLevel.Warning).Should().HaveCount(exceptions.Count * 2);
    }

    [Test, AutoData]
    public async Task CreateProvider_IdempotentPolicyOnNonTransientError_Throw(
        string table, string column, object value, TestRecord testRecord, InvalidOperationException exception)
    {
        // Arrange
        int callCount = 0;
        _connection.SetupQuery(q => true).Returns<TestRecord>(_ => callCount++ == 0 ? throw exception : [testRecord]);

        // Act
        var provider = _sut.CreateProvider(ConnectionString);

        // Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetByIdAsync<TestRecord>(table, column, value).AsTask())
            .Should().Be(exception);

        _logger.Messages.Where(m => m.logLevel == LogLevel.Warning).Should().BeEmpty();
    }

    [Test, AutoData]
    public async Task CreateProvider_NonIdempotentPolicyOnTransientErrors_Retry(TestRecord testRecord)
    {
        // Arrange
        List<Exception> exceptions =
        [
            new TimeoutException(),
            SqlExceptionFactory.CreateSqlException(1205)
        ];

        int callCount = 0;
        _connection.SetupQuery("SELECT 1;")
            .Returns<TestRecord>(_ => exceptions.Count > callCount ? throw exceptions[callCount++] : [testRecord]);

        // Act
        var provider = _sut.CreateProvider(ConnectionString);

        // Assert
        var result = await provider.QuerySingleOrDefaultAsync<TestRecord>("SELECT 1;");
        result.Should().Be(testRecord);

        _logger.Messages.Where(m => m.logLevel == LogLevel.Warning).Should().HaveCount(exceptions.Count * 2);
    }

    [Test, AutoData]
    public async Task CreateProvider_NonIdempotentPolicyWithTimeoutCodeOfSqlException_Throw(TestRecord testRecord)
    {
        // Arrange
        SqlException exception = SqlExceptionFactory.CreateSqlException(-2);

        int callCount = 0;
        _connection.SetupQuery(q => true).Returns<TestRecord>(_ => callCount++ == 0 ? throw exception : [testRecord]);

        // Act
        var provider = _sut.CreateProvider(ConnectionString);

        // Assert
        Assert.ThrowsAsync<SqlException>(() => provider.QuerySingleOrDefaultAsync<TestRecord>("SELECT 1;").AsTask())
            .Should().Be(exception);

        _logger.Messages.Where(m => m.logLevel == LogLevel.Warning).Should().BeEmpty();
    }

    [Test, AutoData]
    public async Task CreateProvider_NonIdempotentPolicyOnNonTransientError_Throw(
        TestRecord testRecord, InvalidOperationException exception)
    {
        // Arrange
        int callCount = 0;
        _connection.SetupQuery(q => true).Returns<TestRecord>(_ => callCount++ == 0 ? throw exception : [testRecord]);

        // Act
        var provider = _sut.CreateProvider(ConnectionString);

        // Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => provider.QuerySingleOrDefaultAsync<TestRecord>("SELECT 1;").AsTask())
            .Should().Be(exception);

        _logger.Messages.Where(m => m.logLevel == LogLevel.Warning).Should().BeEmpty();
    }

    public sealed record TestRecord(Guid Id);

    private sealed class CustomConnectionDbDataProviderFactory(
        DbConnection connection, IOptions<RetryStrategyOptions> options, ILoggerFactory loggerFactory)
        : DbDataProviderFactory(options, loggerFactory)
    {
        private readonly DbConnection _connection = connection;

        protected override DbConnection CreateConnection(string connectionString)
        {
            connectionString.Should().Be(ConnectionString);
            return _connection;
        }
    }
}
