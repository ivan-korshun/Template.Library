// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;
using Template.Library.WebApi.Controllers;

namespace Template.Library.IntegrationTests.OpenApi;

[TestFixture]
public class OpenApiTests : IntegrationTestsBase
{
    private const string ApiVersion = "1";
    private const string Endpoint = $"openapi/v{ApiVersion}.json";

    [Test]
    public async Task OpenApiEndpoint_Always_PathsContainControllerRoutes()
    {
        using HttpClient client = Factory.CreateClient();
        using HttpResponseMessage response = await client.GetAsync(Endpoint);

        OpenApiDoc openApiDoc = response.Should().HaveSuccessStatusCode().ContentAs<OpenApiDoc>();

        openApiDoc.Paths.Keys.Should().BeEquivalentTo(GetControllersEndpoints(
            typeof(AuthorsController),
            typeof(BooksController),
            typeof(BranchesController)));
    }

    protected List<string> GetControllersEndpoints(params Type[] controllers)
    {
        IEnumerable<EndpointDataSource> endpointSources =
            Factory.Services.GetService(typeof(IEnumerable<EndpointDataSource>)) as IEnumerable<EndpointDataSource>
            ?? throw new InvalidOperationException("No controller endpoints available");

        return endpointSources
            .SelectMany(es => es.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(e =>
            {
                var controllerDescriptor = e.Metadata.OfType<ControllerActionDescriptor>().FirstOrDefault();
                string? type = controllerDescriptor?.ControllerTypeInfo.FullName;

                return e.RoutePattern.RawText != null
                    && controllers.Any(t => t.FullName == type)
                    && e.RoutePattern.RawText.Contains("{version:apiVersion}");
            })
            .Select(e => $"/{e.RoutePattern.RawText!.Replace("{version:apiVersion}", ApiVersion)}")
            .Distinct()
            .OrderBy(p => p)
            .ToList();
    }

    private record OpenApiDoc(Dictionary<string, object?> Paths);
}
