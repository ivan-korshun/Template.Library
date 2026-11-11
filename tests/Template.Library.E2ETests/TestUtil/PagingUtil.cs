// Copyright © Korshun Ivan. All rights reserved.

using System.Runtime.CompilerServices;
using Template.Library.E2ETests.TestModels;

namespace Template.Library.E2ETests.TestUtil;

public static class PagingUtil
{
    public static async IAsyncEnumerable<TResult> GoThroughCheckpointPagesAsync<TResult>(
        Func<string?, Task<CheckpointPage<TResult>>> getNextPageFunc,
        int perPage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        string? from = null;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CheckpointPage<TResult> page = await getNextPageFunc(from);

            if (page == null)
            {
                yield break;
            }

            foreach (TResult result in page.Data)
            {
                yield return result;
            }

            from = page.Next;
            if (page.Data.Count < perPage)
            {
                yield break;
            }
        }
    }

    public static async IAsyncEnumerable<TResult> GoThroughPagesAsync<TResult>(
        Func<int, Task<ICollection<TResult>>> getNextPageFunc,
        int perPage,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        int pageNumber = 1;
        while (true)
        {
            token.ThrowIfCancellationRequested();
            ICollection<TResult> pagedList = await getNextPageFunc(pageNumber);

            if (pagedList == null)
            {
                yield break;
            }

            foreach (TResult result in pagedList)
            {
                yield return result;
            }

            pageNumber++;
            if (pagedList.Count < perPage)
            {
                yield break;
            }
        }
    }
}
