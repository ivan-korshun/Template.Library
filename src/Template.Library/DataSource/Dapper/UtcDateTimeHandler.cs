// Copyright © Korshun Ivan. All rights reserved.

using System.Data;
using Dapper;

namespace Template.Library.DataSource.Dapper;

public sealed class UtcDateTimeHandler : SqlMapper.TypeHandler<DateTime>
{
    public override DateTime Parse(object value)
    {
        return value is DateTime dateTime
            ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
            : throw new DataException($"Cannot map {value.GetType().FullName} to {nameof(DateTime)}.");
    }

    public override void SetValue(IDbDataParameter parameter, DateTime value)
    {
        parameter.Value = value;
    }
}
