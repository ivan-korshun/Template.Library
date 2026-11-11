// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using System.Net.Http.Json;
using NUnit.Framework;
using Template.Library.E2ETests.TestModels;
using Template.Library.E2ETests.TestUtil;

namespace Template.Library.E2ETests.Api;

[TestFixture]
public class BookModelValidationTests : ApiTestsBase
{
    private const int MaxPageCount = 32000;

    private static string BooksEndpoint => Constants.Endpoints.Books;

    [TestCase("Query", "")]
    [TestCase("Post", "")]
    [TestCase("Patch", "/55555555555555555555555555555555")]
    public async Task Endpoint_NoBodyWhenRequired_UnsupportedMediaType(string method, string urlSuffix)
    {
        using var request = new HttpRequestMessage(HttpMethod.Parse(method), BooksEndpoint + urlSuffix);

        using HttpResponseMessage response = await SutClient.SendAsync(request);

        response.Should().HaveUnsupportedMediaTypeProblem();
    }

    [TestCase("Query", "")]
    [TestCase("Post", "")]
    [TestCase("Patch", "/55555555555555555555555555555555")]
    public async Task Endpoint_EmptyStringBodyWhenRequired_BadRequest(string method, string urlSuffix)
    {
        using var request = new HttpRequestMessage(HttpMethod.Parse(method), BooksEndpoint + urlSuffix)
        {
            Content = JsonContent.Create("")
        };

        using HttpResponseMessage response = await SutClient.SendAsync(request);

        response.Should().HaveBadRequestProblem("", ["The input was not valid."]);
    }

    [TestCase("Query", "")]
    [TestCase("Post", "")]
    [TestCase("Patch", "/55555555555555555555555555555555")]
    public async Task Endpoint_BrokenJsonBodyWhenRequired_BadRequest(string method, string urlSuffix)
    {
        using var request = new HttpRequestMessage(HttpMethod.Parse(method), BooksEndpoint + urlSuffix)
        {
            Content = JsonContent.Create("{\"xxx")
        };

        using HttpResponseMessage response = await SutClient.SendAsync(request);

        response.Should().HaveBadRequestProblem("", ["The input was not valid."]);
    }

    [Test]
    public async Task Create_NoRequiredFields_BadRequest()
    {
        using HttpResponseMessage response = await SutClient.PostAsJsonAsync(BooksEndpoint, new { });

        response.Should().HaveBadRequestProblem(new Dictionary<string, string[]>
        {
            [nameof(Book.Title)] = [$"The {nameof(Book.Title)} field is required."],
            [nameof(Book.Genre)] = [$"The {nameof(Book.Genre)} field is required."],
            [nameof(Book.Authors)] = [$"The {nameof(Book.Authors)} field is required."],
            [nameof(Book.PageCount)] = [$"The field {nameof(Book.PageCount)} must be between 1 and {MaxPageCount}."]
        });
    }

    [TestCase("Title", 3.3)]
    [TestCase("Genre", 3.3)]
    [TestCase("PageCount", "ups")]
    [TestCase("Available", "ups")]
    [TestCase("AddedAt", "ups")]
    [TestCase("Authors", 3.3)]
    [TestCase("Unknown", "any")]
    public async Task Create_BrokenFieldFormat_BadRequest(string name, object value)
    {
        using HttpResponseMessage response = await SutClient.PostAsJsonAsync(
            BooksEndpoint, new Dictionary<string, object> { [name] = value });

        response.Should().HaveBadRequestProblem(name, ["The input was not valid."]);
    }

    [Test]
    public async Task Create_BrokenAuthorFormat_BadRequest()
    {
        double number = 3.3;

        using HttpResponseMessage response = await SutClient.PostAsJsonAsync(
            BooksEndpoint, new Dictionary<string, object> { ["Authors"] = new[] { number } });

        response.Should().HaveBadRequestProblem("Authors[0]", ["The input was not valid."]);
    }

    [TestCase("Title", "", "The Title field is required.")]
    [TestCase("Title", " ", "The Title field is required.")]
    [TestCase("Title", String256, "The field Title must be a string or array type with a maximum length of '255'.")]
    [TestCase("Genre", -1, "The field Genre is invalid.")]
    [TestCase("Genre", 0, "The field Genre is invalid.")]
    [TestCase("Genre", int.MaxValue, "The field Genre is invalid.")]
    [TestCase("Genre", "NonExisting", "The input was not valid.")]
    [TestCase("PageCount", -1, "The field PageCount must be between 1 and 32000.")]
    [TestCase("PageCount", 0, "The field PageCount must be between 1 and 32000.")]
    [TestCase("PageCount", MaxPageCount + 1, "The field PageCount must be between 1 and 32000.")]
    [TestCase(
        "Authors",
        new[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a" },
        "The field Authors must be a string or array type with a maximum length of '20'.")]
    public async Task Create_InvalidFieldValue_BadRequest(string name, object? value, string error)
    {
        var body = new Dictionary<string, object?>
        {
            ["Title"] = "Book",
            ["Genre"] = "Fantasy",
            ["PageCount"] = MaxPageCount,
            ["Authors"] = Array.Empty<string>()
        };

        body[name] = value;

        using HttpResponseMessage response = await SutClient.PostAsJsonAsync(BooksEndpoint, body);

        response.Should().HaveBadRequestProblem(name, [error]);
    }

    [TestCase(null, "The Authors field must be a collection which elements are not null or empty.")]
    [TestCase("", "The Authors field must be a collection which elements are not null or empty.")]
    [TestCase(" ", "The Authors field must be a collection which elements are not null or empty.")]
    [TestCase(String256, "The maximum length of collection element must be '255' for the Authors field.")]
    public async Task Create_InvalidAuthorValue_BadRequest(object? value, string error)
    {
        var body = new Dictionary<string, object?>
        {
            ["Title"] = "Book",
            ["Genre"] = "Fantasy",
            ["PageCount"] = MaxPageCount,
            ["Authors"] = new[] { value }
        };

        using HttpResponseMessage response = await SutClient.PostAsJsonAsync(BooksEndpoint, body);

        response.Should().HaveBadRequestProblem("Authors", [error]);
    }

    [TestCase("Title", 3.3)]
    [TestCase("Genre", 3.3)]
    [TestCase("PageCount", "ups")]
    [TestCase("Available", "ups")]
    [TestCase("AddedAt", "ups")]
    [TestCase("Authors", 3.3)]
    [TestCase("Unknown", "any")]
    public async Task Update_BrokenFieldFormat_BadRequest(string name, object value)
    {
        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BooksEndpoint}/55555555555555555555555555555555", new Dictionary<string, object> { [name] = value });

        response.Should().HaveBadRequestProblem(name, ["The input was not valid."]);
    }

    [Test]
    public async Task Update_BrokenAuthorFormat_BadRequest()
    {
        double number = 3.3;

        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BooksEndpoint}/55555555555555555555555555555555",
            new Dictionary<string, object> { ["Authors"] = new[] { number } });

        response.Should().HaveBadRequestProblem("Authors[0]", ["The input was not valid."]);
    }

    [Test]
    public async Task Update_BrokenIdFormat_BadRequest()
    {
        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync($"{BooksEndpoint}/ups", new { });

        response.Should().HaveBadRequestProblem("id", ["The value 'ups' is not valid."]);
    }

    [TestCase("Title", "", "The field Title must be a string or array type with a minimum length of '1'.")]
    [TestCase("Title", String256, "The field Title must be a string or array type with a maximum length of '255'.")]
    [TestCase("Genre", -1, "The field Genre is invalid.")]
    [TestCase("Genre", 0, "The field Genre is invalid.")]
    [TestCase("Genre", int.MaxValue, "The field Genre is invalid.")]
    [TestCase("Genre", "NonExisting", "The input was not valid.")]
    [TestCase("PageCount", -1, "The field PageCount must be between 1 and 32000.")]
    [TestCase("PageCount", 0, "The field PageCount must be between 1 and 32000.")]
    [TestCase("PageCount", MaxPageCount + 1, "The field PageCount must be between 1 and 32000.")]
    [TestCase(
    "Authors",
    new[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a" },
    "The field Authors must be a string or array type with a maximum length of '20'.")]
    public async Task Update_InvalidFieldValue_BadRequest(string name, object? value, string error)
    {
        var body = new Dictionary<string, object?>
        {
            ["Title"] = "Book",
            ["Genre"] = "Fantasy",
            ["PageCount"] = MaxPageCount,
            ["Authors"] = Array.Empty<string>()
        };

        body[name] = value;

        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BooksEndpoint}/55555555555555555555555555555555", body);

        response.Should().HaveBadRequestProblem(name, [error]);
    }

    [TestCase(null, "The Authors field must be a collection which elements are not null or empty.")]
    [TestCase("", "The Authors field must be a collection which elements are not null or empty.")]
    [TestCase(" ", "The Authors field must be a collection which elements are not null or empty.")]
    [TestCase(String256, "The maximum length of collection element must be '255' for the Authors field.")]
    public async Task Update_InvalidAuthorValue_BadRequest(object? value, string error)
    {
        var body = new Dictionary<string, object?>
        {
            ["Title"] = "Book",
            ["Genre"] = "Fantasy",
            ["PageCount"] = MaxPageCount,
            ["Authors"] = new[] { value }
        };

        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BooksEndpoint}/55555555555555555555555555555555", body);

        response.Should().HaveBadRequestProblem("Authors", [error]);
    }

    [Test]
    public async Task Delete_BrokenIdFormat_BadRequest()
    {
        using HttpResponseMessage response = await SutClient.DeleteAsync($"{BooksEndpoint}/ups");

        response.Should().HaveBadRequestProblem("id", ["The value 'ups' is not valid."]);
    }

    [Test]
    public async Task QueryCheckpointPage_NoRequiredFields_BadRequest()
    {
        using HttpResponseMessage response = await SutClient.QueryAsJsonAsync(BooksEndpoint, new { });

        response.Should().HaveBadRequestProblem("Limit", [$"The field Limit must be between 1 and {MaxPageSize}."]);
    }

    [TestCase("From", 3.3)]
    [TestCase("Limit", "ups")]
    [TestCase("Title", 3.3)]
    [TestCase("Unknown", 3.3)]
    public async Task QueryCheckpointPage_BrokenFieldFormat_BadRequest(string name, object value)
    {
        using HttpResponseMessage response = await SutClient.QueryAsJsonAsync(
            BooksEndpoint, new Dictionary<string, object> { [name] = value });

        response.Should().HaveBadRequestProblem(name, ["The input was not valid."]);
    }

    [TestCase("From", "", "The field From must be a string or collection type with a minimum length of '1' and maximum length of '255'.")]
    [TestCase("From", String256, "The field From must be a string or collection type with a minimum length of '1' and maximum length of '255'.")]
    [TestCase("From", "wrong", "The value is invalid.")]
    [TestCase("Limit", -1, "The field Limit must be between 1 and 100.")]
    [TestCase("Limit", 0, "The field Limit must be between 1 and 100.")]
    [TestCase("Limit", MaxPageSize + 1, "The field Limit must be between 1 and 100.")]
    [TestCase("Title", "", "The field Title must be a string or collection type with a minimum length of '1' and maximum length of '255'.")]
    [TestCase("Title", String256, "The field Title must be a string or collection type with a minimum length of '1' and maximum length of '255'.")]
    public async Task QueryCheckpointPage_InvalidFieldValue_BadRequest(string name, object? value, string error)
    {
        var body = new Dictionary<string, object?> { ["Limit"] = MaxPageSize };
        body[name] = value;

        using HttpResponseMessage response = await SutClient.QueryAsJsonAsync(BooksEndpoint, body);

        response.Should().HaveBadRequestProblem(name, [error]);
    }

    [Test]
    public async Task GetOffsetPage_NoRequiredParams_BadRequest()
    {
        using HttpResponseMessage response = await SutClient.GetAsync(BooksEndpoint);

        response.Should().HaveBadRequestProblem("PageSize", [$"The field PageSize must be between 1 and {MaxPageSize}."]);
    }

    [Test]
    public async Task GetOffsetPage_BrokenParamsFormat_BadRequest()
    {
        using HttpResponseMessage response = await SutClient.GetAsync($"{BooksEndpoint}?pageNumber=ups&pageSize=ups");

        response.Should().HaveBadRequestProblem(new Dictionary<string, string[]>
        {
            ["PageSize"] = ["The value 'ups' is not valid for PageSize."],
            ["PageNumber"] = ["The value 'ups' is not valid for PageNumber."]
        });
    }

    [TestCase("PageNumber", -1, "The field PageNumber must be between 1 and 20000000.")]
    [TestCase("PageNumber", 0, "The field PageNumber must be between 1 and 20000000.")]
    [TestCase("PageNumber", MaxPageNumber + 1, "The field PageNumber must be between 1 and 20000000.")]
    [TestCase("PageSize", -1, "The field PageSize must be between 1 and 100.")]
    [TestCase("PageSize", 0, "The field PageSize must be between 1 and 100.")]
    [TestCase("PageSize", MaxPageSize + 1, "The field PageSize must be between 1 and 100.")]
    public async Task GetOffsetPage_InvalidParamValue_BadRequest(string name, object? value, string error)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["PageNumber"] = MaxPageNumber,
            ["PageSize"] = MaxPageSize
        };

        parameters[name] = value;

        using HttpResponseMessage response = await SutClient.GetAsync($"{BooksEndpoint}?{parameters.ToQueryString()}");

        response.Should().HaveBadRequestProblem(name, [error]);
    }

    [Test]
    public async Task Get_BrokenIdFormat_BadRequest()
    {
        using HttpResponseMessage response = await SutClient.GetAsync($"{BooksEndpoint}/ups");

        response.Should().HaveBadRequestProblem("id", ["The value 'ups' is not valid."]);
    }
}
