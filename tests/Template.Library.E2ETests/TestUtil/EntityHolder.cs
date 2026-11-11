// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.E2ETests.TestUtil;

public sealed class EntityHolder<T> : IAsyncDisposable
{
    private readonly Func<T, Task> _cleanup;

    public EntityHolder(T entity, Func<T, Task> cleanup)
    {
        _cleanup = cleanup;
        Entity = entity;
    }

    public T Entity { get; }

    public async ValueTask DisposeAsync()
    {
        await _cleanup(Entity);
    }
}
