using AiBloger.Core.Interfaces;
using AiBloger.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiBloger.Infrastructure.Services;

public sealed class JobSchedulerService : BackgroundService
{
    private readonly IScrapeJobQueue _jobQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobSchedulerService> _logger;
    private readonly JobScheduler _options;

    public JobSchedulerService(
        IScrapeJobQueue jobQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<JobSchedulerService> logger,
        IOptions<JobScheduler> options)
    {
        _jobQueue = jobQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Job scheduler started. Will check for new NewsItems every {Interval} seconds, batch size: {BatchSize}",
            _options.CheckIntervalSeconds,
            _options.BatchSize);

        await RecoverInQueueItemsAsync(stoppingToken);
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScheduleJobsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in job scheduler loop");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(_options.CheckIntervalSeconds), 
                stoppingToken);
        }

        _logger.LogInformation("Job scheduler stopped");
    }

    private async Task RecoverInQueueItemsAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recovering InQueue items from previous session...");
        
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.NewsDbContext>();
            
            var inQueueItems = await context.NewsItems
                .Where(x => x.Status == Core.Enums.NewsItemStatus.InQueue)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            if (inQueueItems.Count == 0)
            {
                _logger.LogInformation("No InQueue items found to recover");
                return;
            }

            _logger.LogInformation("Found {Count} InQueue items to recover", inQueueItems.Count);

            var recoveredCount = 0;
            foreach (var newsItem in inQueueItems)
            {
                var job = new ScrapeJob
                {
                    SourceId = newsItem.Id,
                    SourceName = newsItem.Source,
                    Url = newsItem.Url,
                    ArticleTitle = newsItem.Title,
                    RetryCount = newsItem.RetryCount,
                    MaxRetries = _options.MaxRetries
                };

                await _jobQueue.EnqueueAsync(job, cancellationToken);
                recoveredCount++;

                _logger.LogDebug(
                    "Recovered InQueue item #{NewsItemId}: {Title}",
                    newsItem.Id,
                    newsItem.Title);
            }

            _logger.LogInformation(
                "Successfully recovered {Count} InQueue items into the queue", 
                recoveredCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recovering InQueue items");
        }
    }

    private async Task ScheduleJobsAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogDebug("Starting job scheduling cycle");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var newsRepository = scope.ServiceProvider.GetRequiredService<INewsRepository>();

            var newsItems = await newsRepository.GetAndMarkForScrapingAsync(
                _options.BatchSize, 
                cancellationToken);

            if (newsItems.Count == 0)
            {
                _logger.LogDebug("No NewsItems ready for scraping");
                return;
            }

            _logger.LogInformation("Found {Count} NewsItems ready for scraping", newsItems.Count);

            var jobsEnqueued = 0;
            foreach (var newsItem in newsItems)
            {
                var job = new ScrapeJob
                {
                    SourceId = newsItem.Id,
                    SourceName = newsItem.Source,
                    Url = newsItem.Url,
                    ArticleTitle = newsItem.Title,
                    RetryCount = newsItem.RetryCount,
                    MaxRetries = _options.MaxRetries
                };

                await _jobQueue.EnqueueAsync(job, cancellationToken);
                jobsEnqueued++;

                _logger.LogDebug(
                    "Enqueued scrape job #{NewsItemId}: {Title} (Retry: {RetryCount})",
                    newsItem.Id,
                    newsItem.Title,
                    newsItem.RetryCount);
            }

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Job scheduling cycle completed in {Duration}ms. Enqueued {JobCount} jobs. Queue depth: {QueueDepth}",
                duration.TotalMilliseconds,
                jobsEnqueued,
                _jobQueue.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in job scheduling");
        }
    }
}

public class JobScheduler
{
    public int CheckIntervalSeconds { get; set; }

    public int BatchSize { get; set; } = 10;

    public int MaxRetries { get; set; } = 3;
}

