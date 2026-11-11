// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Template.Library.DataSource.Dapper;

namespace Template.Library.DataSource.DB;

public class DatabaseHealthCheck(IDbDataProviderFactory factory, IOptions<DatabaseOptions> options) : IHealthCheck
{
    private readonly IDbDataProviderFactory _factory = factory;
    private readonly string _connectionString = options.Value.ConnectionString;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        await using IDbDataProvider dataProvider = _factory.CreateProvider(_connectionString);

        try
        {
            int? result = await dataProvider.ExecuteScalarAsync<int>("SELECT 1", token: cancellationToken);

            return result == 1
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Unexpected query result.");
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(e.Message, e);
        }
    }
}
