// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.DataSource.Dapper;

public static class SqlConstants
{
    public const string OffsetPaging = "OFFSET @pageSize*(@pageNumber-1) ROWS FETCH NEXT @pageSize ROWS ONLY";
}
