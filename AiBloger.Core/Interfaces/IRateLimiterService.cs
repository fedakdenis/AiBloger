namespace AiBloger.Core.Interfaces;

/// <summary>
/// Service for managing per-source rate limiting
/// </summary>
public interface IRateLimiterService
{
    /// <summary>
    /// Acquires a rate limit permit for the specified source
    /// Returns when permit is available (non-blocking async wait)
    /// </summary>
    ValueTask AcquireAsync(int sourceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets statistics about rate limiter usage
    /// </summary>
    RateLimiterStats GetStats(int sourceId);
}

public record RateLimiterStats(
    int AvailablePermits,
    TimeSpan? RetryAfter
);

