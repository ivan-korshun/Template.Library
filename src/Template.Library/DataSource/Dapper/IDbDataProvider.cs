// Copyright © Korshun Ivan. All rights reserved.

using Dapper;

namespace Template.Library.DataSource.Dapper;

public interface IDbDataProvider : IAsyncDisposable
{
    Task<T> AsTransactionAsync<T>(Func<IDbDataProvider, Task<T>> func, CancellationToken token = default);

    ValueTask<IEnumerable<T>> GetColumnAsync<T>(
        string table, string column, IEnumerable<SqlCondition> where, CancellationToken token = default);

    ValueTask<T?> GetByIdAsync<T>(string table, string idColum, object idValue, CancellationToken token = default);

    Task<PagedResult<T>> GetOffsetPageAsync<T>(
        string table, int pageNumber, int pageSize, string orderBy,
        IEnumerable<SqlCondition>? where = null, bool includeTotal = false,
        CancellationToken token = default);

    Task<PagedResult<T>> GetOffsetPageAsync<T>(
        string table, int pageNumber, int pageSize, string orderBy,
        Action<SqlBuilder> whereAction, bool includeTotal = false,
        CancellationToken token = default);

    Task<PagedResult<T>> GetOffsetPageAsync<T>(
        string table, int pageNumber, int pageSize, string orderBy,
        string whereSql, IEnumerable<KeyValuePair<string, object?>>? whereParams, bool includeTotal = false,
        CancellationToken token = default);

    ValueTask<IEnumerable<T>> GetPageAsync<T>(
        string table, int limit, string fromColumn, object? fromValue, IEnumerable<SqlCondition>? where = null,
        CancellationToken token = default);

    ValueTask<IEnumerable<T>> GetPageAsync<T>(
        string table, int limit, string fromColumn, object? fromValue, Action<SqlBuilder> whereAction,
        CancellationToken token = default);

    ValueTask<IEnumerable<T>> GetPageAsync<T>(
        string table, int limit, string fromColumn, object? fromValue,
        string whereSql, IEnumerable<KeyValuePair<string, object?>>? whereParams = null,
        CancellationToken token = default);

    ValueTask<T?> GetSingleOrDefaultAsync<T>(
        string table, IEnumerable<SqlCondition> where, CancellationToken token = default);

    Task<int> InsertAsync(
        string table, IList<string> columns, IList<IList<object?>> multiValues, CancellationToken token = default);

    Task<T> InsertAsync<T>(
        string table, IEnumerable<KeyValuePair<string, object?>> data, CancellationToken token = default);

    Task<int> InsertIfNotExistsAsync(
        string table, IList<string> columns, IList<IList<object?>> multiValues, IEnumerable<string> matchingColumns,
        CancellationToken token = default);

    Task<T?> UpdateAsync<T>(
        string table, IEnumerable<KeyValuePair<string, object?>> data, IEnumerable<SqlCondition> where,
        CancellationToken token = default);

    Task<T?> UpdateIfNotNullAsync<T>(
        string table, IEnumerable<KeyValuePair<string, object?>> data, IEnumerable<SqlCondition> where,
        CancellationToken token = default);

    ValueTask<int> DeleteAsync(string table, IEnumerable<SqlCondition> where, CancellationToken token = default);

    ValueTask<int> ExecuteAsync(string sql, object? param = null, CancellationToken token = default);

    ValueTask<T?> ExecuteScalarAsync<T>(string sql, object? param = null, CancellationToken token = default);

    ValueTask<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CancellationToken token = default);

    ValueTask<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CancellationToken token = default);

    ValueTask<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? param = null, CancellationToken token = default);
}
