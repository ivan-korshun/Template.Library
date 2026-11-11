// Copyright © Korshun Ivan. All rights reserved.

using System.Text.Json.Serialization;
using Template.Library.E2ETests.TestUtil;

namespace Template.Library.E2ETests.TestModels;

public record Book(
    [property: JsonConverter(typeof(ShortGuidConverter))] Guid? Id = null,
    string? Title = null,
    string? Genre = null,
    int? PageCount = null,
    bool? Available = null,
    DateTime? AddedAt = null,
    ICollection<string>? Authors = null)
{
    public sealed record Disposable : Book, IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await GlobalSetup.SutClient.DeleteAsync($"{Constants.Endpoints.Books}/{Id}");
        }
    }
}
