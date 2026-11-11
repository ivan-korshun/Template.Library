// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Template.Library.IntegrationTests.HealthCheck;

[TestFixture]
public class HealthCheckIntegrationTests : IntegrationTestsBase
{
    private const string HealthZ = "/api/status/healthz";

    [TestCase(HealthZ)]
    [TestCase($"{HealthZ}/auth")]
    [TestCase($"{HealthZ}/credentials")]
    [TestCase($"{HealthZ}/db")]
    [TestCase($"{HealthZ}/org-service")]
    public async Task HealthCheck_DependenciesUp_Healthy(string endpoint)
    {
        // Arrange
        using HttpClient httpClient = Factory.CreateClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(endpoint);

        // Assert
        response.Should().HaveSuccessStatusCode()
            .ContentAsString.Should().Be("Healthy");
    }

    [TestCase(HealthZ)]
    [TestCase($"{HealthZ}/auth")]
    public async Task HealthCheck_AuthenticationDown_Unhealthy(string endpoint)
    {
        // Arrange
        await using var customWebAppFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting($"{AppSection}:JwtBearer:Authority", "https://xxx.xx");
        });

        using HttpClient httpClient = customWebAppFactory.CreateClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(endpoint);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.ServiceUnavailable)
            .ContentAsString.Should().Be("Unhealthy");
    }

    [TestCase(HealthZ)]
    [TestCase($"{HealthZ}/credentials")]
    public async Task HealthCheck_IncorrectCredentials_Unhealthy(string endpoint)
    {
        // Arrange
        await using var customWebAppFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting($"{AppSection}:ClientCredentials:InternalApi:TokenEndpoint", "ftp://xxx.xx");
        });

        using HttpClient httpClient = customWebAppFactory.CreateClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(endpoint);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.ServiceUnavailable)
            .ContentAsString.Should().Be("Unhealthy");
    }

    [TestCase(HealthZ)]
    [TestCase($"{HealthZ}/org-service")]
    public async Task HealthCheck_OrganizationServiceDown_Unhealthy(string endpoint)
    {
        // Arrange
        await using var customWebAppFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting($"{AppSection}:OrganizationService:Host", "ftp://xxx.xx");
        });

        using HttpClient httpClient = customWebAppFactory.CreateClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(endpoint);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.ServiceUnavailable)
            .ContentAsString.Should().Be("Unhealthy");
    }

    [TestCase(HealthZ)]
    [TestCase($"{HealthZ}/db")]
    public async Task HealthCheck_DbDown_Unhealthy(string endpoint)
    {
        // Arrange
        await using var customWebAppFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection([new($"{AppSection}:DB:ConnectionString", "xxx")]);
            });
        });

        using HttpClient httpClient = customWebAppFactory.CreateClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(endpoint);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.ServiceUnavailable)
            .ContentAsString.Should().Be("Unhealthy");
    }
}
