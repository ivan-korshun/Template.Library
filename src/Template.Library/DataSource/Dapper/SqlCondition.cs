// Copyright © Korshun Ivan. All rights reserved.

using System.Text;
using Template.Library.Common;

namespace Template.Library.DataSource.Dapper;

public class SqlCondition
{
    private readonly object? _value;

    public SqlCondition(string column, object value) : this(column, SqlOperator.Equal, value)
    {
    }

    public SqlCondition(string column, SqlOperator sqlOperator, object? value = null)
    {
        Column = column;
        Operator = sqlOperator;
        _value = value;
    }

    public string Column { get; }

    public SqlOperator Operator { get; }

    public object? Value
    {
        get => Operator switch
        {
            SqlOperator.StartsWith => EscapeLike(_value).Append('%').ToString(),
            SqlOperator.EndsWith => EscapeLike(_value).Insert(0, '%').ToString(),
            SqlOperator.Contains => EscapeLike(_value).Insert(0, '%').Append('%').ToString(),
            _ => _value
        };
    }

    public string ToStringCondition()
    {
        return Operator switch
        {
            SqlOperator.Equal => $"[{Column}] = @{Column}",
            SqlOperator.NotEqual => $"[{Column}] <> @{Column}",
            SqlOperator.Less => $"[{Column}] < @{Column}",
            SqlOperator.LessOrEqual => $"[{Column}] <= @{Column}",
            SqlOperator.Greater => $"[{Column}] > @{Column}",
            SqlOperator.GreaterOrEqual => $"[{Column}] >= @{Column}",
            SqlOperator.In => $"[{Column}] IN @{Column}",
            SqlOperator.NotIn => $"[{Column}] NOT IN @{Column}",
            SqlOperator.Like or SqlOperator.StartsWith or SqlOperator.EndsWith or SqlOperator.Contains
                => $"[{Column}] LIKE @{Column}",
            SqlOperator.IsNull => $"[{Column}] IS NULL",
            SqlOperator.IsNotNull => $"[{Column}] IS NOT NULL",
            _ => throw new InvalidOperationException("Wrong SQL operator.")
        };
    }

    private static StringBuilder EscapeLike(object? value)
    {
        string stringValue = ((string?)value).EnsureNotEmpty();

        // Pre-allocate to prevent internal resizing overhead. Assumes 4 encoded symbols.
        var sb = new StringBuilder(stringValue.Length + 4);

        foreach (char c in stringValue)
        {
            if (c is '[' or '%' or '_')
            {
                sb.Append('/');
            }

            sb.Append(c);
        }

        return sb;
    }
}
