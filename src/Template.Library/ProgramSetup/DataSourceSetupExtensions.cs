// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using Template.Library.Common.Http;
using Template.Library.Common.Http.ClientCredentials;
using Template.Library.Common.Http.ClientCredentials.Models;
using Template.Library.Common.Options;
using Template.Library.DataSource.Abstraction;
using Template.Library.DataSource.Dapper;
using Template.Library.DataSource.DB;
using Template.Library.DataSource.Organization;

namespace Template.Library.ProgramSetup;

[ExcludeFromCodeCoverage]
public static class DataSourceSetupExtensions
{
    public static IServiceCollection AddHttpRepositories(this IServiceCollection services, IConfiguration config)
    {
        services.AddAccessTokenManagers(config.GetValidOptions<ClientCredentialsOptions>());

        services.AddValidOptions<OrganizationServiceOptions>(config);
        services.AddAuthorizedHttpClient<OrganizationServiceOptions>(nameof(OrganizationServiceOptions));

        services.AddSingleton<IBranchRepository, BranchRepository>();

        return services;
    }

    public static IServiceCollection AddDbRepositories(
        this IServiceCollection services, IConfiguration config, IConfigurationSection resilienceSection)
    {
        services.AddValidOptions<RetryStrategyOptions>(resilienceSection.GetSection("Retry"));
        services.AddValidOptions<DatabaseOptions>(config);

        SqlMapper.AddTypeHandler(new UtcDateTimeHandler());

        services.AddSingleton<IDbDataProviderFactory, DbDataProviderFactory>();

        services.AddSingleton<IAuthorRepository, AuthorRepository>();
        services.AddSingleton<IBookRepository, BookRepository>();

        return services;
    }

    public static WebApplication LogSqlQueries(this WebApplication app)
    {
        DiagnosticListener.AllListeners.Subscribe(new SqlCommandLoggingObserver(
                app.Services.GetRequiredService<ILogger<SqlCommandLoggingObserver>>()));

        return app;
    }
}
