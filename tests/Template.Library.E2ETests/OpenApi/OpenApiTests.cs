// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using FluentAssertions;
using NUnit.Framework;

namespace Template.Library.E2ETests.OpenApi;

[TestFixture]
public class OpenApiTests
{
    private const string ApiVersion = "1";

    [Test]
    public async Task OpenApiEndpoint_Always_ValidInfoSection()
    {
        using HttpResponseMessage response = await GlobalSetup.GenericClient.GetAsync(
            $"{Settings.LibraryHost}/openapi/v{ApiVersion}.json");

        OpenApiDoc openApiDoc = response.Should().HaveSuccessStatusCode().ContentAs<OpenApiDoc>();

        openApiDoc.Info.Should().BeEquivalentTo(new OpenApiInfo(
            Title: $"Library | v{ApiVersion}",
            Description: "Library management API",
            Version: ApiVersion));
    }

    private record OpenApiDoc(OpenApiInfo Info);

    private record OpenApiInfo(string? Title, string? Description, string? Version);
}
