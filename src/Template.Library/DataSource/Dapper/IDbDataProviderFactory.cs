// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.DataSource.Dapper;

public interface IDbDataProviderFactory
{
    IDbDataProvider CreateProvider(string connectionString);
}
