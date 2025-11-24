using AiBloger.Core.Interfaces;
using AiBloger.Core.Models;
using AiBloger.Core.Enums;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace AiBloger.Infrastructure.Services;

/// <summary>
/// Background service that manages N async workers to process scrape jobs from the queue
/// </summary>
public sealed class ScrapeWorkerService : BackgroundService
{
    private readonly IScrapeJobQueue _jobQueue;
    private readonly IRateLimiterService _rateLimiter;
    private readonly IContentScraperService _contentScraper;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScrapeWorkerService> _logger;
    private readonly ScrapeWorker _options;

    public ScrapeWorkerService(
        IScrapeJobQueue jobQueue,
        IRateLimiterService rateLimiter,
        IContentScraperService contentScraper,
        IServiceScopeFactory scopeFactory,
        ILogger<ScrapeWorkerService> logger,
        IOptions<ScrapeWorker> options)
    {
        _jobQueue = jobQueue;
        _rateLimiter = rateLimiter;
        _contentScraper = contentScraper;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;

        if (_options.Count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(_options.Count));
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Starting {WorkerCount} scrape workers", 
            _options.Count);

        // Start N workers in parallel
        var workers = Enumerable
            .Range(0, _options.Count)
            .Select(workerId => WorkerLoopAsync(workerId, stoppingToken));

        await Task.WhenAll(workers);
        
        _logger.LogInformation("All scrape workers stopped");
    }

    /// <summary>
    /// Worker loop - runs continuously until cancellation
    /// Pure async/await, no blocking, no Task.Run
    /// </summary>
    private async Task WorkerLoopAsync(int workerId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker {WorkerId} started", workerId);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Dequeue job (blocks asynchronously until job is available)
                    var job = await _jobQueue.DequeueAsync(cancellationToken);
                    
                    _logger.LogDebug(
                        "Worker {WorkerId} picked up job for source {SourceName} ({SourceId}): {Url}",
                        workerId, job.SourceName, job.SourceId, job.Url);

                    // Handle the job
                    await HandleJobAsync(job, workerId, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Normal shutdown
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Worker {WorkerId} encountered an error", workerId);
                    
                    // Brief delay before continuing to prevent tight error loop
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
        }
        finally
        {
            _logger.LogInformation("Worker {WorkerId} stopped", workerId);
        }
    }

    private async Task HandleJobAsync(ScrapeJob job, int workerId, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        var newsItemId = job.SourceId;

        try
        {
            _logger.LogDebug(
                "Worker {WorkerId} acquiring rate limit for NewsItem #{NewsItemId}", 
                workerId, newsItemId);
            
            var host = new Uri(job.Url).Host;
            var rateLimitKey = Math.Abs(host.GetHashCode() % 100);
            await _rateLimiter.AcquireAsync(rateLimitKey, cancellationToken);

            _logger.LogDebug(
                "Worker {WorkerId} acquired rate limit for NewsItem #{NewsItemId}", 
                workerId, newsItemId);

            var result = await _contentScraper.ScrapeAsync(job.Url, cancellationToken);

            using var scope = _scopeFactory.CreateScope();
            var newsRepository = scope.ServiceProvider.GetRequiredService<INewsRepository>();
            
            var duration = DateTime.UtcNow - startTime;
            
            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Worker {WorkerId} successfully scraped NewsItem #{NewsItemId} in {Duration}ms: '{Title}' ({ContentLength} chars)",
                    workerId, 
                    newsItemId,
                    duration.TotalMilliseconds,
                    result.Title,
                    result.Content.Length);

                // Обновляем статус на Scraped
                await newsRepository.UpdateStatusAsync(
                    newsItemId,
                    NewsItemStatus.Scraped,
                    scrapedContent: result.Content,
                    errorMessage: null,
                    cancellationToken);
            }
            else
            {
                _logger.LogWarning(
                    "Worker {WorkerId} failed to scrape NewsItem #{NewsItemId}: {Error}",
                    workerId, 
                    newsItemId,
                    result.ErrorMessage);

                var nextStatus = job.RetryCount < job.MaxRetries 
                    ? NewsItemStatus.Retry 
                    : NewsItemStatus.Failed;

                await newsRepository.UpdateStatusAsync(
                    newsItemId,
                    nextStatus,
                    scrapedContent: null,
                    errorMessage: result.ErrorMessage,
                    cancellationToken);
                    
                if (nextStatus == NewsItemStatus.Retry)
                {
                    _logger.LogInformation(
                        "Worker {WorkerId} marked NewsItem #{NewsItemId} for retry ({RetryCount}/{MaxRetries})",
                        workerId, 
                        newsItemId,
                        job.RetryCount + 1,
                        job.MaxRetries);
                }
                else
                {
                    _logger.LogWarning(
                        "Worker {WorkerId} marked NewsItem #{NewsItemId} as Failed (exceeded max retries)",
                        workerId, 
                        newsItemId);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw; // Propagate cancellation
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(
                ex,
                "Worker {WorkerId} failed to handle job for NewsItem #{NewsItemId} after {Duration}ms",
                workerId, 
                newsItemId,
                duration.TotalMilliseconds);
                
            // При критической ошибке пытаемся пометить как Failed
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var newsRepository = scope.ServiceProvider.GetRequiredService<INewsRepository>();
                await newsRepository.UpdateStatusAsync(
                    newsItemId,
                    NewsItemStatus.Failed,
                    scrapedContent: null,
                    errorMessage: $"Critical error: {ex.Message}",
                    cancellationToken);
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update status for NewsItem #{NewsItemId}", newsItemId);
            }
        }
    }
}

/// <summary>
/// Configuration options for scrape workers
/// </summary>
public class ScrapeWorker
{
    public int Count { get; set; }
}
