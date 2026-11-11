// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.Common.Threading;

public sealed class AsyncLock : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _semaphore.Dispose();
        _disposed = true;
    }

    public async ValueTask<Releaser> LockAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        return new Releaser(_semaphore);
    }

    public readonly struct Releaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        internal Releaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            _semaphore?.Release();
        }
    }
}

/* Example:
private readonly AsyncLock _asyncLock = new();

public async Task ProcessDataAsync()
{
    using (await _asyncLock.LockAsync())
    {
        await Task.Delay(1000);
    }
}
*/
