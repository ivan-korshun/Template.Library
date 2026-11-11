// Copyright © Korshun Ivan. All rights reserved.

using System.Text.Json.Serialization;
using Template.Library.E2ETests.TestUtil;

namespace Template.Library.E2ETests.TestModels;

public record Author(
    [property: JsonConverter(typeof(ShortGuidConverter))] Guid? Id = null,
    string? FullName = null,
    string? Country = null,
    DateOnly? Birthday = null,
    bool? Famous = null)
{
    public sealed record Disposable : Author, IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await GlobalSetup.SutClient.DeleteAsync($"{Constants.Endpoints.Authors}/{Id}");
        }
    }
}
