// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using NUnit.Framework;

namespace Template.Library.IntegrationTests.API;

[TestFixture]
public class AuthorsControllerTests : ControllerTestsBase
{
    private const string Endpoint = "api/v1/authors";

    [TestCase("authors:r", "Get", "?pageSize=0")]
    [TestCase("authors:w", "Get", "?pageSize=0")]
    [TestCase("authors:r", "Get", "?limit=0")]
    [TestCase("authors:w", "Get", "?limit=0")]
    [TestCase("authors:r", "Get", "/0")]
    [TestCase("authors:w", "Get", "/0")]
    [TestCase("authors:w", "Post", "")]
    [TestCase("authors:w", "Patch", "/0")]
    [TestCase("authors:w", "Delete", "/0")]
    public async Task Endpoints_MatchingScope_Authorized(string scope, string method, string urlSuffix)
    {
        await ShouldBeAuthorized(scope, HttpMethod.Parse(method), Endpoint + urlSuffix, new { });
    }

    [TestCase("some:w", "Get", "?pageSize=0")]
    [TestCase("some:w", "Get", "?limit=0")]
    [TestCase("some:w", "Get", "/0")]
    [TestCase("authors:r", "Post", "")]
    [TestCase("authors:r", "Patch", "/0")]
    [TestCase("authors:r", "Delete", "/0")]
    public async Task Endpoints_InsufficientScope_Forbidden(string scope, string method, string urlSuffix)
    {
        await ShouldBeForbidden(scope, HttpMethod.Parse(method), Endpoint + urlSuffix, new { });
    }
}
