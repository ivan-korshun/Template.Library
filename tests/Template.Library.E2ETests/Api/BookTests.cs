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
public class BookTests : ApiTestsBase
{
    private readonly List<Author.Disposable> _authors = [];
    private readonly List<Book.Disposable> _books = [];
    private readonly List<string> _bookETags = [];

    private static string BooksEndpoint => Constants.Endpoints.Books;

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _bookETags.Clear();
        await _books.DisposeAsync();
        await _authors.DisposeAsync();
    }

    [TestCase("Get", "")]
    [TestCase("Get", "/55555555555555555555555555555555")]
    [TestCase("Query", "")]
    [TestCase("Post", "")]
    [TestCase("Patch", "/55555555555555555555555555555555")]
    [TestCase("Delete", "/55555555555555555555555555555555")]
    public async Task Endpoint_NoBearerToken_Unauthorized(string method, string urlSuffix)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Parse(method), string.Concat(Settings.LibraryHost, BooksEndpoint, urlSuffix));

        using HttpResponseMessage response = await GlobalSetup.GenericClient.SendAsync(request);

        response.Should().HaveUnauthorizedStatusCode().ContentAsString.Should().BeEmpty();
    }

    [Test]
    public async Task CRUD_MinRequiredFields_Success()
    {
        // Create with required fields only
        var bookRequest = new Book(
            Title: EntityUniqueness.GenerateTitle("MinFields"),
            Genre: "Fantasy",
            PageCount: 78,
            Authors: []);

        using HttpResponseMessage createdResponse = await SutClient.PostAsJsonAsync(
            BooksEndpoint, bookRequest, SkipNulls);

        await using var createdBook = createdResponse.Should().HaveCreatedStatusCode().ContentAs<Book.Disposable>();
        createdBook.Should().BeEquivalentTo(bookRequest with { Available = false }, o => o.Excluding(b => b.Id));
        createdBook.Id.Should().NotBeEmpty();

        createdResponse.Should().HaveLocation($"{BooksEndpoint}/{createdBook.Id}");
        string? etag = createdResponse.Should().HaveEtagHeader();

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BooksEndpoint}/{createdBook.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Book>().Should().BeEquivalentTo(createdBook);
            response.Should().HaveEtagHeaderSameAs(etag);
        }

        // Update a single field
        Book updatedBook = createdBook with { AddedAt = new DateTime(2025, 11, 07, 15, 08, 51) };

        using (HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BooksEndpoint}/{createdBook.Id}", new { AddedAt = updatedBook.AddedAt.Value.AddMilliseconds(123) }))
        {
            response.Should().HaveOkStatusCode().ContentAs<Book>().Should().BeEquivalentTo(updatedBook);
            etag = response.Should().HaveEtagHeaderDifferentFrom(etag);
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BooksEndpoint}/{createdBook.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Book>().Should().BeEquivalentTo(updatedBook);
            response.Should().HaveEtagHeaderSameAs(etag);
        }

        // Delete
        using (HttpResponseMessage response = await SutClient.DeleteAsync($"{BooksEndpoint}/{createdBook.Id}"))
        {
            response.Should().HaveNoContentStatusCode().ContentAsString.Should().BeEmpty();
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BooksEndpoint}/{createdBook.Id}"))
        {
            response.Should().HaveNotFoundProblem();
        }
    }

    [Test]
    public async Task CRUD_AllFields_Success()
    {
        await InitThreeAuthors();

        // Create with required fields only
        var bookRequest = new Book(
            Title: EntityUniqueness.GenerateTitle("AllFields"),
            Genre: "Fantasy",
            PageCount: 78,
            Available: true,
            AddedAt: new DateTime(2025, 11, 07, 15, 08, 51),
            Authors: [.. _authors.Select(a => a.FullName!)]);

        using HttpResponseMessage createdResponse = await SutClient.PostAsJsonAsync(
            BooksEndpoint, bookRequest, SkipNulls);

        await using var createdBook = createdResponse.Should().HaveCreatedStatusCode().ContentAs<Book.Disposable>();
        createdBook.Should().BeEquivalentTo(bookRequest, o => o.Excluding(b => b.Id));
        createdBook.Id.Should().NotBeEmpty();

        createdResponse.Should().HaveLocation($"{BooksEndpoint}/{createdBook.Id}");
        string? etag = createdResponse.Should().HaveEtagHeader();

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BooksEndpoint}/{createdBook.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Book>().Should().BeEquivalentTo(createdBook);
            response.Should().HaveEtagHeaderSameAs(etag);
        }

        // Update all allowed fields
        bookRequest = new Book(
            Title: createdBook.Title + "2",
            Genre: "Comedy",
            PageCount: 782,
            Available: false,
            AddedAt: new DateTime(2002, 02, 2, 22, 22, 2, 123),
            Authors: [_authors[0].FullName!]);

        Book updatedBook = bookRequest with { Id = createdBook.Id, AddedAt = new DateTime(2002, 02, 2, 22, 22, 2) };

        using (HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BooksEndpoint}/{createdBook.Id}", etag, bookRequest, SkipNulls))
        {
            response.Should().HaveOkStatusCode().ContentAs<Book>().Should().BeEquivalentTo(updatedBook);
            etag = response.Should().HaveEtagHeaderDifferentFrom(etag);
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BooksEndpoint}/{createdBook.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Book>().Should().BeEquivalentTo(updatedBook);
            response.Should().HaveEtagHeaderSameAs(etag);
        }

        // Delete
        using (HttpResponseMessage response = await SutClient.DeleteAsync($"{BooksEndpoint}/{createdBook.Id}"))
        {
            response.Should().HaveNoContentStatusCode().ContentAsString.Should().BeEmpty();
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BooksEndpoint}/{createdBook.Id}"))
        {
            response.Should().HaveNotFoundProblem();
        }
    }

    [Test]
    public async Task Update_NoFieldsPassed_OK()
    {
        await InitFourBooks();

        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BooksEndpoint}/{_books[0].Id}", new { });

        response.Should().HaveOkStatusCode().ContentAs<Book>().Should().BeEquivalentTo(_books[0]);
    }

    [Test]
    public async Task Update_NonMatchingEtag_PreconditionFailed()
    {
        await InitFourBooks();

        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BooksEndpoint}/{_books[0].Id}", _bookETags[1], new { });

        response.Should().HavePreconditionFailedProblem();
    }

    [Test]
    public async Task Update_NonExisting_NotFound()
    {
        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BooksEndpoint}/{Guid.NewGuid()}", new { });

        response.Should().HaveNotFoundProblem();
    }

    [Test]
    public async Task Delete_NonExisting_NotFound()
    {
        using HttpResponseMessage response = await SutClient.DeleteAsync($"{BooksEndpoint}/{Guid.NewGuid()}");

        response.Should().HaveNotFoundProblem();
    }

    [Test]
    public async Task GetOffsetPage_ValidParameters_Success()
    {
        // Arrange
        int pageSize = 2;
        await InitFourBooks();

        // Get first page
        using (HttpResponseMessage response = await SutClient.GetAsync(
            $"{BooksEndpoint}?pageNumber=1&pageSize={pageSize}"))
        {
            OffsetPage<Book> page = response.Should().HaveOkStatusCode().ContentAs<OffsetPage<Book>>();
            page.PageNumber.Should().Be(1);
            page.PageSize.Should().Be(pageSize);
            page.Total.Should().BeNull();
            page.Data.Count.Should().Be(pageSize);

            foreach (Book book in page.Data)
            {
                book.Title.Should().NotBeNullOrEmpty();
                book.Genre.Should().NotBeNullOrEmpty();
                book.PageCount.Should().BePositive();
                book.Authors.Should().NotBeNull();
            }
        }

        // Get second page with total
        using (HttpResponseMessage response = await SutClient.GetAsync(
            $"{BooksEndpoint}?pageNumber=2&pageSize={pageSize}&includeTotal=true"))
        {
            OffsetPage<Book> page = response.Should().HaveOkStatusCode().ContentAs<OffsetPage<Book>>();
            page.PageNumber.Should().Be(2);
            page.PageSize.Should().Be(pageSize);
            page.Total.Should().BeGreaterThanOrEqualTo(4);
            page.Data.Count.Should().Be(2);

            foreach (Book book in page.Data)
            {
                book.Title.Should().NotBeNullOrEmpty();
                book.Genre.Should().NotBeNullOrEmpty();
                book.PageCount.Should().BePositive();
                book.Authors.Should().NotBeNull();
            }
        }
    }

    [Test]
    public async Task GetOffsetPage_NonExistingPageNumber_EmptyPage()
    {
        // Act
        using HttpResponseMessage response = await SutClient.GetAsync(
            $"{BooksEndpoint}?pageNumber={MaxPageNumber}&pageSize={MaxPageSize}");

        // Assert
        response.Should().HaveOkStatusCode().ContentAs<OffsetPage<Book>>().Should()
            .BeEquivalentTo(new OffsetPage<Book>([], MaxPageNumber, MaxPageSize));
    }

    [Test]
    public async Task QueryCheckpointPage_ValidParameters_Success()
    {
        // Arrange
        await InitFourBooks();

        // Act: Get first page
        using HttpResponseMessage response1 = await SutClient.QueryAsJsonAsync(BooksEndpoint, new { limit = 2 });

        // Assert
        CheckpointPage<Book> page1 = response1.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Book>>();
        page1.From.Should().BeNull();
        page1.Next.Should().NotBeNullOrEmpty();
        page1.Data.Count.Should().Be(2);

        foreach (Book book in page1.Data)
        {
            book.Title.Should().NotBeNullOrEmpty();
            book.Genre.Should().NotBeNullOrEmpty();
            book.PageCount.Should().BePositive();
            book.Authors.Should().NotBeNull();
        }

        // Act: Get second page
        using HttpResponseMessage response2 = await SutClient.QueryAsJsonAsync(
            BooksEndpoint, new { from = page1.Next, limit = 1 });

        // Assert
        CheckpointPage<Book> page2 = response2.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Book>>();
        page2.From.Should().Be(page1.Next);
        page2.Next.Should().NotBeNullOrEmpty();
        page2.Data.Count.Should().Be(1);

        foreach (Book book in page2.Data)
        {
            book.Title.Should().NotBeNullOrEmpty();
            book.Genre.Should().NotBeNullOrEmpty();
            book.PageCount.Should().BePositive();
            book.Authors.Should().NotBeNull();
        }
    }

    [Test]
    public async Task QueryCheckpointPage_NonExistingFrom_EmptyPage()
    {
        // Arrange
        string from = int.MaxValue.ToString(CultureInfo.InvariantCulture);

        // Act
        using HttpResponseMessage response = await SutClient.QueryAsJsonAsync(
            BooksEndpoint, new { from, limit = MaxPageSize });

        // Assert
        response.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Book>>()
            .Should().BeEquivalentTo(new CheckpointPage<Book>([], from));
    }

    [Test]
    public async Task QuerySearchPage_ValidParameters_Success()
    {
        // Arrange
        await InitFourBooks();

        // Act: Get first page
        using HttpResponseMessage response1 = await SutClient.QueryAsJsonAsync(
            BooksEndpoint, new { limit = 2, title = SearchKey });

        // Assert
        CheckpointPage<Book> page1 = response1.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Book>>();
        page1.Next.Should().NotBeNullOrEmpty();
        page1.Should().BeEquivalentTo(
            new CheckpointPage<Book>([.. _books[0..2].Cast<Book>()], null, null),
            o => o.Excluding(p => p.Next));

        //// Act: Get second page
        using HttpResponseMessage response2 = await SutClient.QueryAsJsonAsync(
            BooksEndpoint, new { from = page1.Next, limit = 1, title = SearchKey });

        // Assert
        CheckpointPage<Book> page2 = response2.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Book>>();
        page2.Next.Should().NotBeNullOrEmpty();
        page2.Should().BeEquivalentTo(
            new CheckpointPage<Book>([_books[2]], page1.Next, null),
            o => o.Excluding(p => p.Next));
    }

    [Test]
    public async Task QuerySearchPage_NonExistingFrom_EmptyPage()
    {
        // Arrange
        string from = int.MaxValue.ToString(CultureInfo.InvariantCulture);

        // Act
        using HttpResponseMessage response = await SutClient.QueryAsJsonAsync(
            BooksEndpoint, new { from, limit = MaxPageSize, title = SearchKey });

        // Assert
        response.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Book>>()
            .Should().BeEquivalentTo(new CheckpointPage<Book>([], from));
    }

    [Test]
    public async Task QuerySearchPage_NonFound_EmptyPage()
    {
        // Act
        using HttpResponseMessage response = await SutClient.QueryAsJsonAsync(
            BooksEndpoint, new { limit = MaxPageSize, title = "non-existing" });

        // Assert
        response.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Book>>()
            .Should().BeEquivalentTo(new CheckpointPage<Book>([]));
    }

    private async Task InitThreeAuthors()
    {
        if (_authors.Count > 0)
        {
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            using HttpResponseMessage createdResponse = await SutClient.PostAsJsonAsync(
               Constants.Endpoints.Authors, new { FullName = EntityUniqueness.GenerateFullName() });

            _authors.Add(createdResponse.Should().HaveCreatedStatusCode().ContentAs<Author.Disposable>());
        }
    }

    private async Task InitFourBooks()
    {
        if (_books.Count != 0)
        {
            return;
        }

        await InitThreeAuthors();

        for (int i = 0; i < 4; i++)
        {
            using HttpResponseMessage createdResponse = await SutClient.PostAsJsonAsync(
                Constants.Endpoints.Books,
                new Book(
                    Title: EntityUniqueness.GenerateTitle(SearchKey),
                    Genre: "Fantasy",
                    PageCount: 100 + i,
                    Authors: [_authors[i % 2].FullName!]),
                SkipNulls);

            _books.Add(createdResponse.Should().HaveCreatedStatusCode().ContentAs<Book.Disposable>());
            _bookETags.Add(createdResponse.Should().HaveEtagHeader());
        }
    }
}
