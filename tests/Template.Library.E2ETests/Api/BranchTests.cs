// Copyright © Korshun Ivan. All rights reserved.

using System.Net.Http;
using System.Net.Http.Json;
using FluentAssertions;
using NUnit.Framework;
using Template.Library.E2ETests.TestModels;
using Template.Library.E2ETests.TestUtil;

namespace Template.Library.E2ETests.Api;

[TestFixture]
public class BranchTests : ApiTestsBase
{
    private readonly List<Branch.Disposable> _branches = [];

    private static string BranchesEndpoint => Constants.Endpoints.Branches;

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _branches.DisposeAsync();
    }

    [TestCase("Get", "")]
    [TestCase("Get", "/100000")]
    [TestCase("Post", "")]
    [TestCase("Patch", "/100000")]
    [TestCase("Delete", "/100000")]
    public async Task Endpoint_NoBearerToken_Unauthorized(string method, string urlSuffix)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Parse(method), string.Concat(Settings.LibraryHost, BranchesEndpoint, urlSuffix));

        using HttpResponseMessage response = await GlobalSetup.GenericClient.SendAsync(request);

        response.Should().HaveUnauthorizedStatusCode().ContentAsString.Should().BeEmpty();
    }

    [Test]
    public async Task CRUD_MinRequiredFields_Success()
    {
        // Create with required fields only
        var branchRequest = new Branch(
            Country: "UA",
            City: EntityUniqueness.GenerateCity("Dnipro"),
            Street: "Victory",
            BuildingNumber: 7,
            AddedAt: DateTime.UtcNow);

        using HttpResponseMessage createdResponse = await SutClient.PostAsJsonAsync(
            BranchesEndpoint, branchRequest, SkipNulls);

        await using var createdBranch = createdResponse.Should().HaveCreatedStatusCode().ContentAs<Branch.Disposable>();
        createdResponse.Should().HaveLocation($"{BranchesEndpoint}/{createdBranch.Id}");
        createdBranch.Should().BeEquivalentTo(branchRequest, o => o.Excluding(b => b.Id));
        createdBranch.Id.Should().BePositive();

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BranchesEndpoint}/{createdBranch.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Branch>().Should().BeEquivalentTo(createdBranch);
        }

        // Update a single field
        Branch updatedBranch = createdBranch with { Phone = "222222222222" };

        using (HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BranchesEndpoint}/{createdBranch.Id}", new { updatedBranch.Phone }))
        {
            response.Should().HaveOkStatusCode().ContentAs<Branch>().Should().BeEquivalentTo(updatedBranch);
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BranchesEndpoint}/{createdBranch.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Branch>().Should().BeEquivalentTo(updatedBranch);
        }

        // Delete
        using (HttpResponseMessage response = await SutClient.DeleteAsync($"{BranchesEndpoint}/{createdBranch.Id}"))
        {
            response.Should().HaveNoContentStatusCode().ContentAsString.Should().BeEmpty();
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BranchesEndpoint}/{createdBranch.Id}"))
        {
            response.Should().HaveNotFoundProblem();
        }
    }

    [Test]
    public async Task CRUD_AllFields_Success()
    {
        // Create with required fields only
        var branchRequest = new Branch(
            Country: "UA",
            City: EntityUniqueness.GenerateCity("Dnipro"),
            Street: "Victory",
            BuildingNumber: 7,
            AddedAt: DateTime.UtcNow,
            Phone: "111111111111");

        using HttpResponseMessage createdResponse = await SutClient.PostAsJsonAsync(
            BranchesEndpoint, branchRequest, SkipNulls);

        await using var createdBranch = createdResponse.Should().HaveCreatedStatusCode().ContentAs<Branch.Disposable>();
        createdResponse.Should().HaveLocation($"{BranchesEndpoint}/{createdBranch.Id}");
        createdBranch.Should().BeEquivalentTo(branchRequest, o => o.Excluding(b => b.Id));
        createdBranch.Id.Should().BePositive();

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BranchesEndpoint}/{createdBranch.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Branch>().Should().BeEquivalentTo(createdBranch);
        }

        // Update all allowed fields
        branchRequest = new Branch(
            Country: "US",
            City: createdBranch.City + "2",
            Street: "Victory2",
            BuildingNumber: 72,
            Phone: "222222222222");

        Branch updatedBranch = branchRequest with { Id = createdBranch.Id, AddedAt = createdBranch.AddedAt };

        using (HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BranchesEndpoint}/{createdBranch.Id}", branchRequest, SkipNulls))
        {
            response.Should().HaveOkStatusCode().ContentAs<Branch>().Should().Be(updatedBranch);
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BranchesEndpoint}/{createdBranch.Id}"))
        {
            response.Should().HaveOkStatusCode().ContentAs<Branch>().Should().Be(updatedBranch);
        }

        // Delete
        using (HttpResponseMessage response = await SutClient.DeleteAsync($"{BranchesEndpoint}/{createdBranch.Id}"))
        {
            response.Should().HaveNoContentStatusCode().ContentAsString.Should().BeEmpty();
        }

        using (HttpResponseMessage response = await SutClient.GetAsync($"{BranchesEndpoint}/{createdBranch.Id}"))
        {
            response.Should().HaveNotFoundProblem();
        }
    }

    [Test]
    public async Task Update_NoFieldsPassed_OK()
    {
        await InitFourBranches();

        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BranchesEndpoint}/{_branches[0].Id}", new { });

        response.Should().HaveOkStatusCode().ContentAs<Branch>().Should().BeEquivalentTo(_branches[0]);
    }

    [Test]
    public async Task Update_NonExisting_NotFound()
    {
        using HttpResponseMessage response = await SutClient.PatchAsJsonAsync(
            $"{BranchesEndpoint}/{1_000_000_000}", new { });

        response.Should().HaveNotFoundProblem();
    }

    [Test]
    public async Task Delete_NonExisting_NotFound()
    {
        using HttpResponseMessage response = await SutClient.DeleteAsync($"{BranchesEndpoint}/{1_000_000_000}");

        response.Should().HaveNotFoundProblem();
    }

    [Test]
    public async Task GetAll_Always_AllBranches()
    {
        // Arrange
        await InitFourBranches();

        using HttpResponseMessage response = await SutClient.GetAsync(BranchesEndpoint);

        Page<Branch> page = response.Should().HaveOkStatusCode().ContentAs<Page<Branch>>();
        page.Data.Count.Should().BeGreaterThanOrEqualTo(4);
        page.Data.Where(b => b.City!.Contains(SearchKey)).Should().BeEquivalentTo(_branches);
    }

    private async Task InitFourBranches()
    {
        if (_branches.Count > 0)
        {
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            using HttpResponseMessage createdResponse = await SutClient.PostAsJsonAsync(
                Constants.Endpoints.Branches,
                new Branch(
                    Country: "UA",
                    City: EntityUniqueness.GenerateCity(SearchKey),
                    Street: "Test Street" + i,
                    BuildingNumber: 10 + i,
                    AddedAt: DateTime.UtcNow),
               SkipNulls);

            _branches.Add(createdResponse.Should().HaveCreatedStatusCode().ContentAs<Branch.Disposable>());
        }
    }
}
