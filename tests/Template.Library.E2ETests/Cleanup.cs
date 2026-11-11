// Copyright © Korshun Ivan. All rights reserved.

using System.Net;
using System.Net.Http;
using NUnit.Framework;
using Template.Library.E2ETests.TestModels;
using Template.Library.E2ETests.TestUtil;

namespace Template.Library.E2ETests;

[TestFixture]
[Category(Constants.Categories.NotForProd)]
public class Cleanup
{
    private const int PageSize = 100;

    private static HttpClient SutClient => GlobalSetup.SutClient;

    [Test]
    public async Task CleanupTestBranches()
    {
        using var pageResponse = await SutClient.GetAsync($"{Constants.Endpoints.Branches}");
        ICollection<Branch> branches = pageResponse.Should().HaveOkStatusCode().ContentAs<Page<Branch>>().Data;

        foreach (Branch branch in branches.Where(b => EntityUniqueness.IsLeftoverCity(b.City!)))
        {
            using HttpResponseMessage response = await SutClient.DeleteAsync($"{Constants.Endpoints.Branches}/{branch.Id}");
            response.Should().HaveAnyOfStatusCodes([HttpStatusCode.NoContent, HttpStatusCode.NotFound]);
            await TestContext.Progress.WriteLineAsync($"The branch '{branch.Id}' deleted with {nameof(Cleanup)}.");
        }
    }

    [Test]
    public async Task CleanupTestAuthors()
    {
        List<Author> authors = await PagingUtil.GoThroughCheckpointPagesAsync(
            static async from =>
            {
                using var response = await SutClient.GetAsync(
                    $"{Constants.Endpoints.Authors}?from={from}&limit={PageSize}&fullName={Constants.UniquePrefix}");

                return response.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Author>>();
            },
            PageSize).ToListAsync();

        foreach (Author author in authors.Where(b => EntityUniqueness.IsLeftoverFullName(b.FullName!)))
        {
            using HttpResponseMessage response = await SutClient.DeleteAsync($"{Constants.Endpoints.Authors}/{author.Id}");
            response.Should().HaveAnyOfStatusCodes([HttpStatusCode.NoContent, HttpStatusCode.NotFound]);
            await TestContext.Progress.WriteLineAsync($"The author '{author.Id}' deleted with {nameof(Cleanup)}.");
        }
    }

    [Test]
    public async Task CleanupTestBooks()
    {
        List<Book> books = await PagingUtil.GoThroughCheckpointPagesAsync(
            static async from =>
            {
                using var response = await SutClient.QueryAsJsonAsync(
                    Constants.Endpoints.Books, new { from, Limit = PageSize, Title = Constants.UniquePrefix });

                return response.Should().HaveOkStatusCode().ContentAs<CheckpointPage<Book>>();
            },
            PageSize).ToListAsync();

        foreach (Book book in books.Where(b => EntityUniqueness.IsLeftoverTitle(b.Title!)))
        {
            using HttpResponseMessage response = await SutClient.DeleteAsync($"{Constants.Endpoints.Books}/{book.Id}");
            response.Should().HaveAnyOfStatusCodes([HttpStatusCode.NoContent, HttpStatusCode.NotFound]);
            await TestContext.Progress.WriteLineAsync($"The book '{book.Id}' deleted with {nameof(Cleanup)}.");
        }
    }
}
