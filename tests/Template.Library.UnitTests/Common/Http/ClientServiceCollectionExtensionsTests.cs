// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using AutoFixture.NUnit4;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Template.Library.Common.Http;
using Template.Library.Common.Http.ClientCredentials;

namespace Template.Library.UnitTests.Common.Http;

public class ClientServiceCollectionExtensionsTests
{

    [Test, AutoData]
    public async Task AddHttpClientBasics_Always_AddUserAgentToAllClients(string userAgent)
    {
        // Arrange
        var services = new ServiceCollection();

        var data = new Dictionary<string, string?>
        {
            ["Resilience:AttemptTimeout:Timeout"] = "00:00:01",
        };

        var resilienceSection = new ConfigurationBuilder().AddInMemoryCollection(data).Build().GetSection("Resilience");

        // Act
        services.AddHttpClientBasics(resilienceSection, userAgent);

        // Assert
        var clientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        using HttpClient httpClient = clientFactory.CreateClient();

        httpClient.DefaultRequestHeaders.UserAgent.ToString().Should().Be(userAgent);
    }

    [Test, AutoData]
    public void AddStaticHttpClient_Always_RegistersHttpClient(string clientName)
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddStaticHttpClient(clientName);

        // Assert
        IHttpClientFactory clientFactory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
        using HttpClient httpClient = clientFactory.CreateClient(clientName);

        httpClient.Should().NotBeNull();
    }

    [Test, AutoData]
    public void AddAuthorizedHttpClient_Always_RegistersHttpClient(string clientName)
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.Services.AddKeyedSingleton(clientName, (_, _) => Substitute.For<IAccessTokenManager>());

        builder.Services.AddTransient(_ => Options.Create(new TestOptions
        {
            Host = "https://example.com",
            ClientCredentials = clientName
        }));

        // Act
        builder.Services.AddAuthorizedHttpClient<TestOptions>(clientName);

        // Assert
        IHttpClientFactory clientFactory = builder.Build().Services.GetRequiredService<IHttpClientFactory>();
        using HttpClient httpClient = clientFactory.CreateClient(clientName);

        httpClient.Should().NotBeNull();
        httpClient.BaseAddress.Should().Be("https://example.com");
    }

    [Test, AutoData]
    public void AddAuthorizedHttpClient_Always_ConfiguresAccessTokenManager(string clientName)
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();

        builder.Services.AddTransient(_ => Options.Create(new TestOptions
        {
            Host = "https://example.com",
            ClientCredentials = clientName
        }));

        // Act
        builder.Services.AddAuthorizedHttpClient<TestOptions>(clientName);

        // Assert
        IHttpClientFactory clientFactory = builder.Build().Services.GetRequiredService<IHttpClientFactory>();

        Action action = () => clientFactory.CreateClient(clientName);

        action.Should().Throw<InvalidOperationException>().WithMessage(
            "No keyed service for type '*.IAccessTokenManager' using key type 'System.String' has been registered.");
    }

    private class TestOptions : ServiceOptionsBase;
}
