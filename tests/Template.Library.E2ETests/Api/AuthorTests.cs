// Copyright © Korshun Ivan. All rights reserved.

using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using FluentAssertions;
using NUnit.Framework;
using Template.Library.E2ETests.TestModels;
using Template.Library.E2ETests.TestUtil;

namespace Template.Library.E2ETests.Api;

[TestFixture]
public class AuthorTests : ApiTestsBase
{
    private readonly List<Author.Disposable> _authors = [];
    private readonly List<string> _authorETags = [];

    private static string AuthorsEndpoint => Constants.Endpoints.Authors;

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _authorETags.Clear();
        await _authors.DisposeAsync();
    }

    [TestCase("Get", "?pageSize=1")]
    [TestCase("Get", "?limit=1")]
    [TestCase("Get", "/55555555555555555555555555555555")]
    [TestCase("Post", "")]
    [TestCase("Patch", "/55555555555555555555555555555555")]
    [TestCase("Delete", "/55555555555555555555555555555555")]
    public async Task Endpoint_NoBearerToken_Unauthorized(string method, string urlSuffix)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Parse(method), string.Concat(Settings.LibraryHost, AuthorsEndpoint, urlSuffix));

        using HttpResponseMessage response = await GlobalSetup.GenericClient.SendAsync(request);

        response.Should().HaveUnauthorizedStatusCode().ContentAsString.Should().BeEmpty();
    }

    [Test]
    public async Task CRUD_MinRequiredFields_Success()
    {
        // Create with required fields only
        var authorRequest = new Author(
            FullName: EntityUniqueness.GenerateFullName("MinFields"));

        using HttpResponseMessage createdResponse = await SutClient.PostAsJsonAsync(
            AuthorsEndpoint, authorRequest, SkipNulls);

        await using var createdAuthor = createdResponse.Should().HaveCreatedStatusCode().ContentAs<Author.Disposable>();
        createdAuthor.Should().BeEquivalentTo(authorRequest with { Famous = false }, o => o.Excluding(b => b.Id));
        createdAuthor.Id.Should().NotBeEmpty();

        createdResponse.Should().HaveLocation($"{AuthorsEndpoint}/{createdAuthor.Id}");
        string? etag = createdResponse.Should().HaveEtagHeader();

        using (HttpResponseMessage response = await SutClient.GetAsync($"{AuthorsEndpoint}/{createdAuthor.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Author>().Should().BeEquivalentTo(createdAuthor);
            response.Should().HaveEtagHeaderSameAs(etag);
        }

        // Update a single field
        Author updatedAuthor = createdAuthor with { Birthday = new DateOnly(2000, 7, 21) };

        using (HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{AuthorsEndpoint}/{createdAuthor.Id}", new { updatedAuthor.Birthday }))
        {
            response.Should().HaveOkStatusCode().ContentAs<Author>().Should().BeEquivalentTo(updatedAuthor);
            etag = response.Should().HaveEtagHeaderDifferentFrom(etag);
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{AuthorsEndpoint}/{createdAuthor.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Author>().Should().BeEquivalentTo(updatedAuthor);
            response.Should().HaveEtagHeaderSameAs(etag);
        }

        // Delete
        using (HttpResponseMessage response = await SutClient.DeleteAsync($"{AuthorsEndpoint}/{createdAuthor.Id}"))
        {
            response.Should().HaveNoContentStatusCode().ContentAsString.Should().BeEmpty();
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{AuthorsEndpoint}/{createdAuthor.Id}"))
        {
            response.Should().HaveNotFoundProblem();
        }
    }

    [Test]
    public async Task CRUD_AllFields_Success()
    {
        // Create with required fields only
        var authorRequest = new Author(
            FullName: EntityUniqueness.GenerateFullName("AllFields"),
            Country: "UA",
            Birthday: new DateOnly(2001, 1, 1),
            Famous: true);

        using HttpResponseMessage createdResponse = await SutClient.PostAsJsonAsync(
            AuthorsEndpoint, authorRequest, SkipNulls);

        await using var createdAuthor = createdResponse.Should().HaveCreatedStatusCode().ContentAs<Author.Disposable>();
        createdAuthor.Should().BeEquivalentTo(authorRequest, o => o.Excluding(b => b.Id));
        createdAuthor.Id.Should().NotBeEmpty();

        createdResponse.Should().HaveLocation($"{AuthorsEndpoint}/{createdAuthor.Id}");
        string? etag = createdResponse.Should().HaveEtagHeader();

        using (HttpResponseMessage response = await SutClient.GetAsync($"{AuthorsEndpoint}/{createdAuthor.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Author>().Should().BeEquivalentTo(createdAuthor);
            response.Should().HaveEtagHeaderSameAs(etag);
        }

        // Update all allowed fields
        authorRequest = new Author(
            FullName: createdAuthor.FullName + "2",
            Country: "US",
            Birthday: new DateOnly(2002, 2, 2),
            Famous: false);

        Author updatedAuthor = authorRequest with { Id = createdAuthor.Id };

        using (HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{AuthorsEndpoint}/{createdAuthor.Id}", etag, authorRequest, SkipNulls))
        {
            response.Should().HaveOkStatusCode().ContentAs<Author>().Should().BeEquivalentTo(updatedAuthor);
            etag = response.Should().HaveEtagHeaderDifferentFrom(etag);
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{AuthorsEndpoint}/{createdAuthor.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Author>().Should().BeEquivalentTo(updatedAuthor);
            response.Should().HaveEtagHeaderSameAs(etag);
        }

        // Delete
        using (HttpResponseMessage response = await SutClient.DeleteAsync($"{AuthorsEndpoint}/{createdAuthor.Id}"))
        {
            response.Should().HaveNoContentStatusCode().ContentAsString.Should().BeEmpty();
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{AuthorsEndpoint}/{createdAuthor.Id}"))
        {
            response.Should().HaveNotFoundProblem();
        }
    }

    [Test]
    public async Task Create_SetExistingFullName_Conflict()
    {
        await InitFourAuthors();

        using HttpResponseMessage response = await SutClient.PostAsJsonAsync(
            AuthorsEndpoint, new { fullName = _authors[0].FullName });

        response.Should().HaveConflictProblem($"Cannot set duplicate value '{_authors[0].FullName}'.");
    }

    [Test]
    public async Task Update_SetExistingFullName_Conflict()
    {
        await InitFourAuthors();

        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{AuthorsEndpoint}/{_authors[1].Id}", new { fullName = _authors[0].FullName });

        response.Should().HaveConflictProblem($"Cannot set duplicate value '{_authors[0].FullName}'.");
    }

    [Test]
    public async Task Update_NoFieldsPassed_OK()
    {
        await InitFourAuthors();

        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{AuthorsEndpoint}/{_authors[0].Id}", new { });

        response.Should().HaveOkStatusCode().ContentAs<Author>().Should().BeEquivalentTo(_authors[0]);
    }

    [Test]
    public async Task Update_NonMatchingEtag_PreconditionFailed()
    {
        await InitFourAuthors();

        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{AuthorsEndpoint}/{_authors[0].Id}", _authorETags[1], new { });

        response.Should().HavePreconditionFailedProblem();
    }

    [Test]
    public async Task Update_NonExisting_NotFound()
    {
        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{AuthorsEndpoint}/{Guid.NewGuid()}", new { });

        response.Should().HaveNotFoundProblem();
    }

    [Test]
    public async Task Delete_NonExisting_NotFound()
    {
        using HttpResponseMessage response = await SutClient.DeleteAsync($"{AuthorsEndpoint}/{Guid.NewGuid()}");

        response.Should().HaveNotFoundProblem();
    }

    [Test]
    public async Task GetOffsetPage_ValidParameters_Success()
    {
        // Arrange
        int pageSize = 2;
        await InitFourAuthors();

        // Get first page
        using (HttpResponseMessage response = await SutClient.GetAsync(
            $"{AuthorsEndpoint}?pageNumber=1&pageSize={pageSize}"))
        {
            OffsetPage<Author> page = response.Should().HaveOkStatusCode().ContentAs<OffsetPage<Author>>();
            page.PageNumber.Should().Be(1);
            page.PageSize.Should().Be(pageSize);
            page.Total.Should().BeNull();
            page.Data.Count.Should().Be(pageSize);

            foreach (Author author in page.Data)
            {
                author.FullName.Should().NotBeNullOrEmpty();
            }
        }

        // Get second page with total
        using (HttpResponseMessage response = await SutClient.GetAsync(
            $"{AuthorsEndpoint}?pageNumber=2&pageSize={pageSize}&includeTotal=true"))
        {
            OffsetPage<Author> page = response.Should().HaveOkStatusCode().ContentAs<OffsetPage<Author>>();
            page.PageNumber.Should().Be(2);
            page.PageSize.Should().Be(pageSize);
            page.Total.Should().BeGreaterThanOrEqualTo(4);
            page.Data.Count.Should().Be(2);

            foreach (Author author in page.Data)
            {
                author.FullName.Should().NotBeNullOrEmpty();
            }
        }
    }

    [Test]
    public async Task GetOffsetPage_NonExistingPageNumber_EmptyPage()
    {
        // Act
        using HttpResponseMessage response = await SutClient.GetAsync(
            $"{AuthorsEndpoint}?pageNumber={MaxPageNumber}&pageSize={MaxPageSize}");

        // Assert
        response.Should().HaveOkStatusCode().ContentAs<OffsetPage<Author>>().Should()
            .BeEquivalentTo(new OffsetPage<Author>([], MaxPageNumber, MaxPageSize));
    }

    [Test]
    public async Task GetCheckpointPage_ValidParameters_Success()
    {
        // Arrange
        await InitFourAuthors();

        // Act: Get first page
        using HttpResponseMessage response1 = await SutClient.GetAsync($"{AuthorsEndpoint}?limit=2");

        // Assert
        CheckpointPage<Author> page1 = response1.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Author>>();
        page1.From.Should().BeNull();
        page1.Next.Should().NotBeNullOrEmpty();
        page1.Data.Count.Should().Be(2);

        foreach (Author author in page1.Data)
        {
            author.FullName.Should().NotBeNullOrEmpty();
        }

        // Act: Get second page
        using HttpResponseMessage response2 = await SutClient.GetAsync($"{AuthorsEndpoint}?from={page1.Next}&limit=1");

        // Assert
        CheckpointPage<Author> page2 = response2.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Author>>();
        page2.From.Should().Be(page1.Next);
        page2.Next.Should().NotBeNullOrEmpty();
        page2.Data.Count.Should().Be(1);

        foreach (Author author in page2.Data)
        {
            author.FullName.Should().NotBeNullOrEmpty();
        }
    }

    [Test]
    public async Task GetCheckpointPage_NonExistingFrom_EmptyPage()
    {
        // Arrange
        string from = int.MaxValue.ToString(CultureInfo.InvariantCulture);

        // Act
        using HttpResponseMessage response = await SutClient.GetAsync(
            $"{AuthorsEndpoint}?from={from}&limit={MaxPageSize}");

        // Assert
        response.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Author>>()
            .Should().BeEquivalentTo(new CheckpointPage<Author>([], from));
    }

    [Test]
    public async Task GetSearchPage_ValidParameters_Success()
    {
        // Arrange
        await InitFourAuthors();

        // Act: Get first page
        using HttpResponseMessage response1 = await SutClient.GetAsync(
            $"{AuthorsEndpoint}?limit=2&fullName={SearchKey}");

        // Assert
        CheckpointPage<Author> page1 = response1.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Author>>();
        page1.Next.Should().NotBeNullOrEmpty();
        page1.Should().BeEquivalentTo(
            new CheckpointPage<Author>([.. _authors[0..2].Cast<Author>()], null, null),
            o => o.Excluding(p => p.Next));

        //// Act: Get second page
        using HttpResponseMessage response2 = await SutClient.GetAsync(
            $"{AuthorsEndpoint}?from={page1.Next}&limit=1&fullName={SearchKey}");

        // Assert
        CheckpointPage<Author> page2 = response2.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Author>>();
        page2.Next.Should().NotBeNullOrEmpty();
        page2.Should().BeEquivalentTo(
            new CheckpointPage<Author>([_authors[2]], page1.Next, null),
            o => o.Excluding(p => p.Next));
    }

    [Test]
    public async Task GetSearchPage_NonExistingFrom_EmptyPage()
    {
        // Arrange
        string from = int.MaxValue.ToString(CultureInfo.InvariantCulture);

        // Act
        using HttpResponseMessage response = await SutClient.GetAsync(
            $"{AuthorsEndpoint}?from={from}&limit={MaxPageSize}&fullName={SearchKey}");

        // Assert
        response.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Author>>()
            .Should().BeEquivalentTo(new CheckpointPage<Author>([], from));
    }

    [Test]
    public async Task GetSearchPage_NonFound_EmptyPage()
    {
        // Act
        using HttpResponseMessage response = await SutClient.GetAsync(
            $"{AuthorsEndpoint}?limit={MaxPageSize}&fullName=non-existing");

        // Assert
        response.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Author>>()
            .Should().BeEquivalentTo(new CheckpointPage<Author>([]));
    }

    private async Task InitFourAuthors()
    {
        if (_authors.Count > 0)
        {
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            using HttpResponseMessage createdResponse = await SutClient.PostAsJsonAsync(
                Constants.Endpoints.Authors,
                new { FullName = EntityUniqueness.GenerateFullName(SearchKey) });

            _authors.Add(createdResponse.Should().HaveCreatedStatusCode().ContentAs<Author.Disposable>());
            _authorETags.Add(createdResponse.Should().HaveEtagHeader());
        }
    }
}
