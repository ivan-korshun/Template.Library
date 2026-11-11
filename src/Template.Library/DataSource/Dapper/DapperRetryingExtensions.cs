// Copyright © Korshun Ivan. All rights reserved.

using System.Data;
using Dapper;
using Polly;

namespace Template.Library.DataSource.Dapper;

public static class DapperRetryingExtensions
{
    public static ValueTask<T?> QuerySingleOrDefaultAsync<T>(
        this IDbConnection connection, ResiliencePipeline retryPolicy, string sql, object? param = null,
        IDbTransaction? transaction = null, CancellationToken token = default)
    {
        return retryPolicy.ExecuteAsync(
            async t => await connection.QuerySingleOrDefaultAsync<T>(
                new CommandDefinition(sql, param, transaction, cancellationToken: t)),
            token);
    }

    public static ValueTask<T> QuerySingleAsync<T>(
        this IDbConnection connection, ResiliencePipeline retryPolicy, string sql, object? param = null,
        IDbTransaction? transaction = null, CancellationToken token = default)
    {
        return retryPolicy.ExecuteAsync(
            async t => await connection.QuerySingleAsync<T>(
                new CommandDefinition(sql, param, transaction, cancellationToken: t)),
            token);
    }

    public static ValueTask<IEnumerable<T>> QueryAsync<T>(
        this IDbConnection connection, ResiliencePipeline retryPolicy, string sql, object? param = null,
        IDbTransaction? transaction = null, CancellationToken token = default)
    {
        return retryPolicy.ExecuteAsync(
            async t => await connection.QueryAsync<T>(
                new CommandDefinition(sql, param, transaction, cancellationToken: t)),
            token);
    }

    public static ValueTask<SqlMapper.GridReader> QueryMultipleAsync(
        this IDbConnection connection, ResiliencePipeline retryPolicy, string sql, object? param = null,
        IDbTransaction? transaction = null, CancellationToken token = default)
    {
        return retryPolicy.ExecuteAsync(
            async t => await connection.QueryMultipleAsync(
                new CommandDefinition(sql, param, transaction, cancellationToken: t)),
            token);
    }

    public static ValueTask<int> ExecuteAsync(
        this IDbConnection connection, ResiliencePipeline retryPolicy, string sql, object? param = null,
        IDbTransaction? transaction = null, CancellationToken token = default)
    {
        return retryPolicy.ExecuteAsync(
            async t => await connection.ExecuteAsync(
                new CommandDefinition(sql, param, transaction, cancellationToken: t)),
            token);
    }

    public static ValueTask<T?> ExecuteScalarAsync<T>(
        this IDbConnection connection, ResiliencePipeline retryPolicy, string sql, object? param = null,
        IDbTransaction? transaction = null, CancellationToken token = default)
    {
        return retryPolicy.ExecuteAsync(
            async t => await connection.ExecuteScalarAsync<T>(
                new CommandDefinition(sql, param, transaction, cancellationToken: t)),
            token);
    }
}
