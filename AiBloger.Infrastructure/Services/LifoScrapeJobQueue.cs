using System.Collections.Concurrent;
using AiBloger.Core.Interfaces;
using AiBloger.Core.Models;
using Microsoft.Extensions.Options;

namespace AiBloger.Infrastructure.Services;

/// <summary>
/// Thread-safe LIFO queue with bounded capacity implemented via ConcurrentStack + SemaphoreSlim.
/// </summary>
public sealed class LifoScrapeJobQueue : IScrapeJobQueue, IDisposable
{
    private readonly ConcurrentStack<ScrapeJob> _stack = new();
    private readonly SemaphoreSlim _itemsAvailable;
    private readonly SemaphoreSlim _spaceAvailable;
    private bool _disposed;

    public LifoScrapeJobQueue(IOptions<ScrapeQueue> scrapeQueue)
    {
        var capacity = scrapeQueue.Value.Capacity;
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive");
        }

        _itemsAvailable = new SemaphoreSlim(initialCount: 0, maxCount: capacity);
        _spaceAvailable = new SemaphoreSlim(initialCount: capacity, maxCount: capacity);
    }

    public async ValueTask EnqueueAsync(ScrapeJob job, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await _spaceAvailable.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            _stack.Push(job);
        }
        catch
        {
            _spaceAvailable.Release();
            throw;
        }

        _itemsAvailable.Release();
    }

    public async ValueTask<ScrapeJob> DequeueAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        await _itemsAvailable.WaitAsync(cancellationToken).ConfigureAwait(false);

        if (_stack.TryPop(out var job))
        {
            _spaceAvailable.Release();
            return job;
        }

        _itemsAvailable.Release();
        throw new InvalidOperationException("Queue state is inconsistent: no item after semaphore signal.");
    }

    public int Count
    {
        get
        {
            ThrowIfDisposed();
            return _itemsAvailable.CurrentCount;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _itemsAvailable.Dispose();
        _spaceAvailable.Dispose();
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(LifoScrapeJobQueue));
        }
    }
}

public class ScrapeQueue
{
    public int Capacity { get; set;}
}


