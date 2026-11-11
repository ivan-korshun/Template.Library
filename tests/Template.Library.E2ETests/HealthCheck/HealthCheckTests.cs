// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Template.Library.E2ETests.HealthCheck;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class HealthCheckTests
{
    private const string HealthZ = "/api/status/healthz";

    [TestCase(HealthZ)]
    [TestCase($"{HealthZ}/started")]
    [TestCase($"{HealthZ}/ready")]
    [TestCase($"{HealthZ}/live")]
    [TestCase($"{HealthZ}/auth")]
    [TestCase($"{HealthZ}/credentials")]
    [TestCase($"{HealthZ}/db")]
    [TestCase($"{HealthZ}/org-service")]
    public async Task ValidateHealthCheckApiUrl(string endpoint)
    {
        using HttpResponseMessage response = await GlobalSetup.GenericClient.GetAsync(Settings.LibraryHost + endpoint);

        // Assert
        response.Should().HaveSuccessStatusCode()
            .ContentAsString.Should().Be("Healthy");
    }
}
