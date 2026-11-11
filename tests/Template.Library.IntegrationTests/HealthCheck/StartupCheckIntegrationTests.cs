// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Template.Library.IntegrationTests;
using Template.Library.WebApi.HealthCheck;

namespace Template.Library.IntegrationTests.HealthCheck;

[TestFixture]
public class StartupCheckIntegrationTests : IntegrationTestsBase
{
    private Uri _startupUri = null!;

    public override async Task OnOneTimeSetUp()
    {
        await base.OnOneTimeSetUp();

        _startupUri = new Uri(
            Factory.Services.GetRequiredService<IOptions<HealthChecksOptions>>().Value.StartedEndpoint,
            UriKind.Relative);
    }

    [Test]
    public async Task StartupCheck_DependenciesUp_Healthy()
    {
        // Arrange
        using HttpClient httpClient = Factory.CreateClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(_startupUri);

        // Assert
        response.Should().HaveSuccessStatusCode()
            .ContentAsString.Should().Be("Healthy");
    }

    [Test]
    public async Task StartupCheck_AuthenticationDown_Unhealthy()
    {
        // Arrange
        await using var customWebAppFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting($"{AppSection}:JwtBearer:Authority", "https://xxx.xx");
        });

        using HttpClient httpClient = customWebAppFactory.CreateClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(_startupUri);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.ServiceUnavailable)
            .ContentAsString.Should().Be("Unhealthy");
    }

    [Test]
    public async Task StartupCheck_IncorrectCredentials_Unhealthy()
    {
        // Arrange
        await using var customWebAppFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting($"{AppSection}:ClientCredentials:InternalApi:TokenEndpoint", "ftp://xxx.xx");
        });

        using HttpClient httpClient = customWebAppFactory.CreateClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(_startupUri);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.ServiceUnavailable)
            .ContentAsString.Should().Be("Unhealthy");
    }

    [Test]
    public async Task ReadinessCheck_OrganizationServiceDown_Unhealthy()
    {
        // Arrange
        await using var customWebAppFactory = Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting($"{AppSection}:OrganizationService:Host", "ftp://xxx.xx");
        });

        using HttpClient httpClient = customWebAppFactory.CreateClient();

        // Act
        using HttpResponseMessage response = await httpClient.GetAsync(_startupUri);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.ServiceUnavailable)
            .ContentAsString.Should().Be("Unhealthy");
    }

    [Test]
    public async Task ReadinessCheck_DbDown_Unhealthy()
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
        using HttpResponseMessage response = await httpClient.GetAsync(_startupUri);

        // Assert
        response.Should().HaveStatusCode(HttpStatusCode.ServiceUnavailable)
            .ContentAsString.Should().Be("Unhealthy");
    }
}
