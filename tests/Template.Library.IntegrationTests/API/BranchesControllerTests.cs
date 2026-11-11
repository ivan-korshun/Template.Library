// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using NUnit.Framework;

namespace Template.Library.IntegrationTests.API;

[TestFixture]
public class BranchesControllerTests : ControllerTestsBase
{
    private const string Endpoint = "api/v1/branches";

    [TestCase("branches:r", "Get", "")]
    [TestCase("branches:w", "Get", "")]
    [TestCase("branches:r", "Get", "/xxx")]
    [TestCase("branches:w", "Get", "/xxx")]
    [TestCase("branches:w", "Post", "")]
    [TestCase("branches:w", "Patch", "/xxx")]
    [TestCase("branches:w", "Delete", "/xxx")]
    public async Task Endpoints_MatchingScope_Authorized(string scope, string method, string urlSuffix)
    {
        await ShouldBeAuthorized(scope, HttpMethod.Parse(method), Endpoint + urlSuffix, new { });
    }

    [TestCase("some:w", "Get", "")]
    [TestCase("some:w", "Get", "/xxx")]
    [TestCase("branches:r", "Post", "")]
    [TestCase("branches:r", "Patch", "/xxx")]
    [TestCase("branches:r", "Delete", "/xxx")]
    public async Task Endpoints_InsufficientScope_Forbidden(string scope, string method, string urlSuffix)
    {
        await ShouldBeForbidden(scope, HttpMethod.Parse(method), Endpoint + urlSuffix, new { });
    }
}
