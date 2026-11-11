// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.E2ETests.TestUtil;

public static class DisposableCollectionExtensions
{
    extension<T>(ICollection<T> collection) where T : IAsyncDisposable
    {
        public async Task DisposeAsync()
        {
            foreach (IAsyncDisposable item in collection)
            {
                await item.DisposeAsync();
            }

            collection.Clear();
        }
    }
}
