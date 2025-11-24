using AiBloger.Core.Models;

namespace AiBloger.Core.Interfaces;

/// <summary>
/// Queue for managing scrape jobs
/// </summary>
public interface IScrapeJobQueue
{
    /// <summary>
    /// Enqueues a job to the queue
    /// </summary>
    ValueTask EnqueueAsync(ScrapeJob job, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Dequeues a job from the queue (blocks until job is available)
    /// </summary>
    ValueTask<ScrapeJob> DequeueAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current queue depth
    /// </summary>
    int Count { get; }
}

