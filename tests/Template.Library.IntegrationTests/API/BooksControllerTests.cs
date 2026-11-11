// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using NUnit.Framework;

namespace Template.Library.IntegrationTests.API;

[TestFixture]
public class BooksControllerTests : ControllerTestsBase
{
    private const string Endpoint = "api/v1/books";

    [TestCase("books:r", "Get", "")]
    [TestCase("books:w", "Get", "")]
    [TestCase("books:r", "Get", "/0")]
    [TestCase("books:w", "Get", "/0")]
    [TestCase("books:r", "Query", "")]
    [TestCase("books:w", "Query", "")]
    [TestCase("books:w", "Post", "")]
    [TestCase("books:w", "Patch", "/0")]
    [TestCase("books:w", "Delete", "/0")]
    public async Task Endpoints_MatchingScope_Authorized(string scope, string method, string urlSuffix)
    {
        await ShouldBeAuthorized(scope, HttpMethod.Parse(method), Endpoint + urlSuffix, new { });
    }

    [TestCase("some:w", "Get", "")]
    [TestCase("some:w", "Get", "/0")]
    [TestCase("some:w", "Query", "")]
    [TestCase("books:r", "Post", "")]
    [TestCase("books:r", "Patch", "/0")]
    [TestCase("books:r", "Delete", "/0")]
    public async Task Endpoints_InsufficientScope_Forbidden(string scope, string method, string urlSuffix)
    {
        await ShouldBeForbidden(scope, HttpMethod.Parse(method), Endpoint + urlSuffix, new { });
    }
}
