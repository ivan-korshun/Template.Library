// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.E2ETests.TestModels;

public record Branch(
    int? Id = null,
    string? Country = null,
    string? City = null,
    string? Street = null,
    int? BuildingNumber = null,
    DateTime? AddedAt = null,
    string? Phone = null)
{
    public sealed record Disposable : Branch, IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await GlobalSetup.SutClient.DeleteAsync($"{Constants.Endpoints.Branches}/{Id}");
        }
    }
}
