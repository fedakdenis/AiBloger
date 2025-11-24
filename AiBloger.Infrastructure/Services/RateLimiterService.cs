using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using AiBloger.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiBloger.Infrastructure.Services;

/// <summary>
/// Manages per-source rate limiters using TokenBucketRateLimiter
/// </summary>
public sealed class RateLimiterService : IRateLimiterService, IDisposable
{
    private readonly ConcurrentDictionary<int, System.Threading.RateLimiting.RateLimiter> _limiters;
    private readonly ILogger<RateLimiterService> _logger;
    private readonly RateLimiter _options;

    public RateLimiterService(
        ILogger<RateLimiterService> logger,
        IOptions<RateLimiter> options)
    {
        _logger = logger;
        _options = options.Value;
        _limiters = new ConcurrentDictionary<int, System.Threading.RateLimiting.RateLimiter>();
    }

    public async ValueTask AcquireAsync(int sourceId, CancellationToken cancellationToken = default)
    {
        var limiter = GetOrCreateLimiter(sourceId);
        
        using var lease = await limiter.AcquireAsync(permitCount: 1, cancellationToken);
        
        if (!lease.IsAcquired)
        {
            _logger.LogError(
                "Failed to acquire rate limit for source {SourceId}. " +
                "Check QueueLimit configuration (current: {QueueLimit}). " +
                "Rejecting request to prevent rate limit violation.",
                sourceId,
                _options.QueueLimit);
            
            throw new InvalidOperationException(
                $"Rate limit could not be acquired for source {sourceId}. " +
                $"Current QueueLimit: {_options.QueueLimit}. " +
                "Either increase QueueLimit to allow waiting, or reduce load.");
        }
    }

    public RateLimiterStats GetStats(int sourceId)
    {
        if (!_limiters.TryGetValue(sourceId, out var limiter))
        {
            return new RateLimiterStats(AvailablePermits: 0, RetryAfter: null);
        }

        var metadata = limiter.GetStatistics();
        return new RateLimiterStats(
            AvailablePermits: (int)(metadata?.CurrentAvailablePermits ?? 0),
            RetryAfter: null
        );
    }

    private System.Threading.RateLimiting.RateLimiter GetOrCreateLimiter(int sourceId)
    {
        return _limiters.GetOrAdd(sourceId, _ =>
        {
            var limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
            {
                TokenLimit = _options.TokenLimit,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = _options.QueueLimit,
                ReplenishmentPeriod = TimeSpan.FromSeconds(_options.ReplenishmentPeriodSeconds),
                TokensPerPeriod = _options.TokensPerPeriod,
                AutoReplenishment = true
            });

            _logger.LogInformation(
                "Created rate limiter for source {SourceId}: {TokenLimit} tokens, replenish {TokensPerPeriod} tokens every {Period}s",
                sourceId, 
                _options.TokenLimit, 
                _options.TokensPerPeriod,
                _options.ReplenishmentPeriodSeconds);

            return limiter;
        });
    }

    public void Dispose()
    {
        foreach (var limiter in _limiters.Values)
        {
            limiter.Dispose();
        }
        _limiters.Clear();
    }
}

/// <summary>
/// Configuration options for rate limiting
/// </summary>
public class RateLimiter
{
    public int TokenLimit { get; set; }
    public int QueueLimit { get; set; }
    public int ReplenishmentPeriodSeconds { get; set; }
    public int TokensPerPeriod { get; set; }
}

