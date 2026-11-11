// Copyright © Korshun Ivan. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Template.Library.Common.Http;
using Template.Library.Common.Http.ClientCredentials;
using Template.Library.Common.Http.HealthChecks;
using Template.Library.DataSource.DB;
using Template.Library.DataSource.Organization;
using Template.Library.WebApi.Auth;
using Template.Library.WebApi.HealthCheck;

namespace Template.Library.ProgramSetup;

[ExcludeFromCodeCoverage]
public static class HealthCheckSetupExtensions
{
    public static IHealthChecksBuilder AddAuthCheck(this IHealthChecksBuilder builder)
    {
        builder.AddCachedHealthCheck(
           "authentication",
            ["health", "auth", "started", "ready"],
            sp => new JwtBearerHealthCheck(
                sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientNames.DefaultStaticClient),
                sp.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme)));

        return builder;
    }

    public static IHealthChecksBuilder AddCredentialsCheck(this IHealthChecksBuilder builder)
    {
        builder.AddCachedHealthCheck(
            "credentials",
            ["health", "credentials", "started", "ready"],
            sp => new CredentialsHealthCheck(sp.GetKeyedServices<IAccessTokenManager>(KeyedService.AnyKey)));

        return builder;
    }

    public static IHealthChecksBuilder AddDbCheck(this IHealthChecksBuilder builder)
    {
        builder.AddCachedHealthCheck<DatabaseHealthCheck>(
            "db",
            ["health", "db", "started", "ready"]);

        return builder;
    }

    public static IHealthChecksBuilder AddOrganizationServiceChecks(this IHealthChecksBuilder builder)
    {
        builder.Services.AddSingleton<IHttpEndpointChecker, HttpEndpointChecker>();

        builder.AddCachedHealthCheck<OrganizationServiceHealthCheck>(
            "org_service_health",
            ["health", "org-service", "started"]);

        builder.AddCachedHealthCheck<OrganizationServiceLifeCheck>(
            "org_service_life",
            ["ready"]);

        return builder;
    }
}
