// Copyright © Korshun Ivan. All rights reserved.

using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace Template.Library.DataSource.Dapper;

public class DbDataProviderFactory : IDbDataProviderFactory
{
    private readonly Lazy<ResiliencePipeline> _idempotentPolicy;
    private readonly Lazy<ResiliencePipeline> _nonIdempotentPolicy;

    public DbDataProviderFactory(IOptions<RetryStrategyOptions> options, ILoggerFactory loggerFactory)
    {
        _idempotentPolicy = new(() => BuildResiliencePipeline(
            options.Value,
            loggerFactory,
            [.. SqlConfigurableRetryFactory.BaselineTransientErrors, -2])); // -2 = Timeout

        _nonIdempotentPolicy = new(() => BuildResiliencePipeline(
            options.Value, loggerFactory, SqlConfigurableRetryFactory.BaselineTransientErrors));
    }

    public IDbDataProvider CreateProvider(string connectionString)
    {
        return new DbDataProvider(
            CreateConnection(connectionString), _idempotentPolicy.Value, _nonIdempotentPolicy.Value);
    }

    protected virtual DbConnection CreateConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }

    private static ResiliencePipeline BuildResiliencePipeline(
        RetryStrategyOptions options, ILoggerFactory loggerFactory, IReadOnlyCollection<int> retryCodes)
    {
        return new ResiliencePipelineBuilder()
            .ConfigureTelemetry(loggerFactory)
            .AddRetry(new RetryStrategyOptions
            {
                BackoffType = options.BackoffType,
                MaxRetryAttempts = options.MaxRetryAttempts,
                Delay = options.Delay,
                UseJitter = options.UseJitter,

                ShouldHandle = new PredicateBuilder()
                        .Handle<TimeoutException>()
                        .Handle<SqlException>(ex => retryCodes.Contains(ex.Number))
            })
            .Build();
    }
}
