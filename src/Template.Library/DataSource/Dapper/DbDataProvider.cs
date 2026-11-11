// Copyright © Korshun Ivan. All rights reserved.

using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Dapper;
using Microsoft.Data.SqlClient;
using Polly;

namespace Template.Library.DataSource.Dapper;

public partial class DbDataProvider(
    DbConnection connection, ResiliencePipeline idempotentRetry, ResiliencePipeline nonIdempotentRetry)
    : IDbDataProvider
{
    private const int UniqueIndexErrorCode = 2601;

    private readonly DbConnection _connection = connection;
    private readonly ResiliencePipeline _idempotentRetry = idempotentRetry;
    private readonly ResiliencePipeline _nonIdempotentRetry = nonIdempotentRetry;

    private bool _disposed;
    private IDbTransaction? _transaction;

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await _connection.DisposeAsync();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// The method is not thread safe.
    /// </summary>
    public async Task<T> AsTransactionAsync<T>(Func<IDbDataProvider, Task<T>> func, CancellationToken token = default)
    {
        await _connection.OpenAsync(token); // open connection manually if you need several Dapper calls or transaction

        await using DbTransaction transaction = await _idempotentRetry.ExecuteAsync(
            _connection.BeginTransactionAsync, token);

        try
        {
            if (Interlocked.CompareExchange(ref _transaction, transaction, null) != null)
            {
                throw new InvalidOperationException(
                    $"{nameof(AsTransactionAsync)} method is not thread safe and cannot be called in parallel.");
            }

            T result = await func(this);

            await _idempotentRetry.ExecuteAsync(async t => await transaction.CommitAsync(t), token);
            return result;
        }
        catch (Exception)
        {
            await _idempotentRetry.ExecuteAsync(async t => await transaction.RollbackAsync(t), default);
            throw;
        }
        finally
        {
            _transaction = null;
        }
    }

    public ValueTask<T?> GetByIdAsync<T>(string table, string idColum, object idValue, CancellationToken token = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add(idColum, idValue);

        string sql = $"SELECT * FROM [{table}] WHERE [{idColum}] = @{idColum}";

        return _connection.QuerySingleOrDefaultAsync<T>(_idempotentRetry, sql, parameters, _transaction, token);
    }

    public ValueTask<T?> GetSingleOrDefaultAsync<T>(
        string table, IEnumerable<SqlCondition> where, CancellationToken token = default)
    {
        var parameters = new DynamicParameters();

        string? whereSql = ProcessConditions(where, parameters);

        string sql = $"SELECT * FROM [{table}]{whereSql}";

        return _connection.QuerySingleOrDefaultAsync<T>(_idempotentRetry, sql, parameters, _transaction, token);
    }

    public ValueTask<IEnumerable<T>> GetColumnAsync<T>(
        string table, string column, IEnumerable<SqlCondition> where, CancellationToken token = default)
    {
        var parameters = new DynamicParameters();

        string? whereSql = ProcessConditions(where, parameters);

        string sql = $"SELECT {column} FROM [{table}]{whereSql}";

        return _connection.QueryAsync<T>(_idempotentRetry, sql, parameters, _transaction, token);
    }

    public async Task<PagedResult<T>> GetOffsetPageAsync<T>(
        string table, int pageNumber, int pageSize, string orderBy,
        IEnumerable<SqlCondition>? where = null,
        bool includeTotal = false,
        CancellationToken token = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("pageNumber", pageNumber);
        parameters.Add("pageSize", pageSize);

        string? whereSql = ProcessConditions(where, parameters);

        string sql = $"SELECT * FROM [{table}]{whereSql} ORDER BY {orderBy} {SqlConstants.OffsetPaging};";

        if (includeTotal)
        {
            using SqlMapper.GridReader multi = await _connection.QueryMultipleAsync(
                _idempotentRetry,
                $"SELECT COUNT(*) FROM [{table}]{whereSql}; {sql}",
                parameters,
                _transaction,
                token);

            int total = await multi.ReadFirstAsync<int>();
            IEnumerable<T> items = await multi.ReadAsync<T>();
            return new PagedResult<T> { Page = items, Total = total };
        }
        else
        {
            var items = await _connection.QueryAsync<T>(_idempotentRetry, sql, parameters, _transaction, token);

            return new PagedResult<T> { Page = items };
        }
    }

    public async Task<PagedResult<T>> GetOffsetPageAsync<T>(
        string table, int pageNumber, int pageSize, string orderBy,
        string whereSql, IEnumerable<KeyValuePair<string, object?>>? whereParams,
        bool includeTotal = false,
        CancellationToken token = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("pageNumber", pageNumber);
        parameters.Add("pageSize", pageSize);

        if (whereParams != null)
        {
            foreach (KeyValuePair<string, object?> pair in whereParams)
            {
                parameters.Add(pair.Key, pair.Value);
            }
        }

        if (!string.IsNullOrEmpty(whereSql))
        {
            whereSql = " WHERE " + whereSql;
        }

        string sql = $"SELECT * FROM [{table}]{whereSql} ORDER BY {orderBy} {SqlConstants.OffsetPaging}";

        if (includeTotal)
        {
            using SqlMapper.GridReader multi = await _connection.QueryMultipleAsync(
                _idempotentRetry,
                $"SELECT COUNT(*) FROM [{table}]{whereSql}; {sql}",
                parameters,
                _transaction,
                token);

            int total = await multi.ReadFirstAsync<int>();
            IEnumerable<T> items = await multi.ReadAsync<T>();
            return new PagedResult<T> { Page = items, Total = total };
        }
        else
        {
            var items = await _connection.QueryAsync<T>(_idempotentRetry, sql, parameters, _transaction, token);
            return new PagedResult<T> { Page = items };
        }
    }

    public async Task<PagedResult<T>> GetOffsetPageAsync<T>(
        string table, int pageNumber, int pageSize, string orderBy,
        Action<SqlBuilder> whereAction,
        bool includeTotal = false,
        CancellationToken token = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("pageNumber", pageNumber);
        parameters.Add("pageSize", pageSize);

        string sql = $"SELECT * FROM [{table}] /**where**/ ORDER BY {orderBy} {SqlConstants.OffsetPaging}";
        if (includeTotal)
        {
            sql = $"SELECT COUNT(*) FROM [{table}] /**where**/; {sql}";
        }

        var builder = new SqlBuilder();
        var template = builder.AddTemplate(sql, parameters);

        whereAction(builder);

        if (includeTotal)
        {
            using SqlMapper.GridReader multi = await _connection.QueryMultipleAsync(
                _idempotentRetry, template.RawSql, template.Parameters, _transaction, token);

            int total = await multi.ReadFirstAsync<int>();
            IEnumerable<T> items = await multi.ReadAsync<T>();
            return new PagedResult<T> { Page = items, Total = total };
        }
        else
        {
            var items = await _connection.QueryAsync<T>(
                _idempotentRetry, template.RawSql, template.Parameters, _transaction, token);

            return new PagedResult<T> { Page = items };
        }
    }

    public ValueTask<IEnumerable<T>> GetPageAsync<T>(
        string table, int limit, string fromColumn, object? fromValue,
        IEnumerable<SqlCondition>? where = null,
        CancellationToken token = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("limit", limit);

        List<string> whereParts = [];
        if (fromValue != null)
        {
            whereParts.Add($"[{fromColumn}] > @{fromColumn}");
            parameters.Add(fromColumn, fromValue);
        }

        if (where != null)
        {
            ProcessConditions(where, parameters, whereParts);
        }

        string? whereSql = JoinWhereParts(whereParts);

        string sql = $"SELECT TOP(@limit) * FROM [{table}]{whereSql} ORDER BY {fromColumn}";

        return _connection.QueryAsync<T>(_idempotentRetry, sql, parameters, _transaction, token);
    }

    public ValueTask<IEnumerable<T>> GetPageAsync<T>(
        string table, int limit, string fromColumn, object? fromValue,
        string whereSql, IEnumerable<KeyValuePair<string, object?>>? whereParams = null,
        CancellationToken token = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("limit", limit);

        if (fromValue != null)
        {
            parameters.Add(fromColumn, fromValue);

            whereSql = $"[{fromColumn}] > @{fromColumn} AND ({whereSql})";
        }

        if (whereParams != null)
        {
            foreach (KeyValuePair<string, object?> pair in whereParams)
            {
                parameters.Add(pair.Key, pair.Value);
            }
        }

        string sql = $"SELECT TOP(@limit) * FROM [{table}] WHERE {whereSql} ORDER BY {fromColumn}";

        return _connection.QueryAsync<T>(_idempotentRetry, sql, parameters, _transaction, token);
    }

    public ValueTask<IEnumerable<T>> GetPageAsync<T>(
        string table, int limit, string fromColumn, object? fromValue,
        Action<SqlBuilder> whereAction,
        CancellationToken token = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("limit", limit);

        string sql = $"SELECT TOP(@limit) * FROM [{table}] /**where**/ ORDER BY {fromColumn}";

        var builder = new SqlBuilder();
        var template = builder.AddTemplate(sql, parameters);

        if (fromValue != null)
        {
            builder.Where($"[{fromColumn}] > @from", new { from = fromValue });
        }

        whereAction(builder);

        return _connection.QueryAsync<T>(_idempotentRetry, template.RawSql, template.Parameters, _transaction, token);
    }

    public async Task<T> InsertAsync<T>(
        string table, IEnumerable<KeyValuePair<string, object?>> data,
        CancellationToken token = default)
    {
        var parameters = new DynamicParameters();
        foreach (KeyValuePair<string, object?> pair in data)
        {
            parameters.Add(pair.Key, pair.Value);
        }

        string columnNames = string.Join(", ", data.Select(p => p.Key));
        string paramNames = string.Join(", ", data.Select(p => "@" + p.Key));

        string sql = $"INSERT INTO {table} ({columnNames}) OUTPUT INSERTED.* VALUES ({paramNames})";

        try
        {
            return await _connection.QuerySingleAsync<T>(_nonIdempotentRetry, sql, parameters, _transaction, token);
        }
        catch (SqlException e)
        {
            throw CreateHighLevelException(e);
        }
    }

    public async Task<int> InsertAsync(
        string table,
        IList<string> columns,
        IList<IList<object?>> multiValues,
        CancellationToken token = default)
    {
        if (columns.Count == 0 || multiValues.Count == 0)
        {
            throw new InvalidOperationException("There should be at least one column, value, and matching column.");
        }

        List<string> valuesParts = [];
        var parameters = new DynamicParameters();

        for (int i = 0; i < multiValues.Count; i++)
        {
            IList<object?> values = multiValues[i];
            valuesParts.Add(string.Join(", ", columns.Select(c => string.Concat("@", c, i))));

            for (int j = 0; j < columns.Count; j++)
            {
                parameters.Add(columns[j] + i, values[j]);
            }
        }

        string insertColumnNames = string.Join(", ", columns);
        string valuesSql = string.Concat("(", string.Join("), (", valuesParts), ")");

        string sql = $"INSERT INTO {table} ({insertColumnNames}) VALUES {valuesSql}";

        try
        {
            return await _connection.ExecuteAsync(_nonIdempotentRetry, sql, parameters, _transaction, token);
        }
        catch (SqlException e)
        {
            throw CreateHighLevelException(e);
        }
    }

    public async Task<int> InsertIfNotExistsAsync(
        string table,
        IList<string> columns,
        IList<IList<object?>> multiValues,
        IEnumerable<string> matchingColumns,
        CancellationToken token = default)
    {
        if (columns.Count == 0 || !matchingColumns.Any() || multiValues.Count == 0)
        {
            throw new InvalidOperationException("There should be at least one column, value, and matching column.");
        }

        List<string> valuesParts = [];
        var parameters = new DynamicParameters();

        for (int i = 0; i < multiValues.Count; i++)
        {
            IList<object?> values = multiValues[i];
            valuesParts.Add(string.Join(", ", columns.Select(c => string.Concat("@", c, i))));

            for (int j = 0; j < columns.Count; j++)
            {
                parameters.Add(columns[j] + i, values[j]);
            }
        }

        string insertColumnNames = string.Join(", ", columns);
        string selectColumnNames = string.Join(", ", columns.Select(c => "s." + c));

        string valuesSql = string.Concat("(", string.Join("), (", valuesParts), ")");

        string whereSql = string.Join(" AND ", matchingColumns.Select(c => $"{table}.{c} = s.{c}"));

        string sql = $"INSERT INTO {table} ({insertColumnNames}) SELECT {selectColumnNames} FROM (VALUES {valuesSql}) AS s ({insertColumnNames}) WHERE NOT EXISTS (SELECT 1 FROM {table} WHERE {whereSql})";

        try
        {
            return await _connection.ExecuteAsync(_nonIdempotentRetry, sql, parameters, _transaction, token);
        }
        catch (SqlException e)
        {
            throw CreateHighLevelException(e);
        }
    }

    public async Task<T?> UpdateAsync<T>(
        string table, IEnumerable<KeyValuePair<string, object?>> data,
        IEnumerable<SqlCondition> where,
        CancellationToken token = default)
    {
        var parameters = new DynamicParameters();

        List<string> dataParts = [];
        foreach (var pair in data)
        {
            dataParts.Add($"[{pair.Key}] = @{pair.Key}");
            parameters.Add(pair.Key, pair.Value);
        }

        if (dataParts.Count == 0)
        {
            return await GetSingleOrDefaultAsync<T>(table, where, token);
        }

        List<string> whereParts = [];
        ProcessConditions(where, parameters, whereParts);

        if (whereParts.Count == 0)
        {
            throw new InvalidOperationException("SQL: WHERE condition is missing on UPDATE.");
        }

        string dataSql = string.Join(", ", dataParts);
        string? whereSql = JoinWhereParts(whereParts);

        string sql = $"UPDATE {table} SET {dataSql} OUTPUT INSERTED.*{whereSql}";

        try
        {
            return await _connection.QuerySingleOrDefaultAsync<T>(
                _nonIdempotentRetry, sql, parameters, _transaction, token);
        }
        catch (SqlException e)
        {
            throw CreateHighLevelException(e);
        }
    }

    public async Task<T?> UpdateIfNotNullAsync<T>(
        string table, IEnumerable<KeyValuePair<string, object?>> data,
        IEnumerable<SqlCondition> where,
        CancellationToken token = default)
    {
        var parameters = new DynamicParameters();

        List<string> dataParts = [];
        foreach (var pair in data.Where(p => p.Value != null))
        {
            dataParts.Add($"[{pair.Key}] = @{pair.Key}");
            parameters.Add(pair.Key, pair.Value);
        }

        if (dataParts.Count == 0)
        {
            return await GetSingleOrDefaultAsync<T>(table, where, token);
        }

        List<string> whereParts = [];
        ProcessConditions(where, parameters, whereParts);

        if (whereParts.Count == 0)
        {
            throw new InvalidOperationException("SQL: WHERE condition is missing on UPDATE.");
        }

        string dataSql = string.Join(", ", dataParts);
        string? whereSql = JoinWhereParts(whereParts);

        string sql = $"UPDATE {table} SET {dataSql} OUTPUT INSERTED.*{whereSql}";

        try
        {
            return await _connection.QuerySingleOrDefaultAsync<T>(
                _nonIdempotentRetry, sql, parameters, _transaction, token);
        }
        catch (SqlException e)
        {
            throw CreateHighLevelException(e);
        }
    }

    public ValueTask<int> DeleteAsync(string table, IEnumerable<SqlCondition> where, CancellationToken token = default)
    {
        var parameters = new DynamicParameters();

        string? whereSql = ProcessConditions(where, parameters);
        if (string.IsNullOrEmpty(whereSql))
        {
            throw new InvalidOperationException("SQL: WHERE condition is missing on DELETE.");
        }

        string sql = $"DELETE FROM {table}{whereSql}";

        return _connection.ExecuteAsync(_nonIdempotentRetry, sql, parameters, _transaction, token);
    }

    public ValueTask<int> ExecuteAsync(string sql, object? param = null, CancellationToken token = default)
    {
        return _connection.ExecuteAsync(_nonIdempotentRetry, sql, param, _transaction, token);
    }

    /// <summary>
    /// Returns	First column of the first row via a plain cast or convert.
    /// </summary>
    public ValueTask<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CancellationToken token = default)
    {
        return _connection.ExecuteScalarAsync<T>(_nonIdempotentRetry, sql, param, _transaction, token);
    }

    public ValueTask<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CancellationToken token = default)
    {
        return _connection.QuerySingleOrDefaultAsync<T>(_nonIdempotentRetry, sql, param, _transaction, token);
    }

    public ValueTask<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken token = default)
    {
        return _connection.QueryAsync<T>(_nonIdempotentRetry, sql, param, _transaction, token);
    }

    public ValueTask<SqlMapper.GridReader> QueryMultipleAsync(
        string sql, object? param = null, CancellationToken token = default)
    {
        return _connection.QueryMultipleAsync(_nonIdempotentRetry, sql, param, _transaction, token);
    }

    private static string? JoinWhereParts(List<string> whereParts)
    {
        return whereParts.Count > 0 ? " WHERE " + string.Join(" AND ", whereParts) : null;
    }

    private static void ProcessConditions(
        IEnumerable<SqlCondition> where, DynamicParameters parameters, List<string> whereParts)
    {
        foreach (SqlCondition condition in where)
        {
            whereParts.Add(condition.ToStringCondition());
            parameters.Add(condition.Column, condition.Value);
        }
    }

    private static string? ProcessConditions(IEnumerable<SqlCondition>? where, DynamicParameters parameters)
    {
        if (where == null)
        {
            return null;
        }

        List<string> sqlParts = [];
        ProcessConditions(where, parameters, sqlParts);
        return JoinWhereParts(sqlParts);
    }

    private static Exception CreateHighLevelException(SqlException e)
    {
        if (e.Number != UniqueIndexErrorCode)
        {
            return e;
        }

        Match match = UniqueErrorRegex().Match(e.Message);
        return new UniqueConstraintException(e)
        {
            DuplicateValue = match.Success ? match.Groups["value"].Value : null,
        };
    }

    [GeneratedRegex(@"The duplicate key value is \((?<value>[^)]+)\)\.")]
    private static partial Regex UniqueErrorRegex();
}
