using System.Diagnostics.Metrics;
using AiBloger.Core.Interfaces;

namespace AiBloger.Infrastructure.Services;

/// <summary>
/// Центральное место объявления бизнес-метрик для конвейера скрейпинга.
/// Использует System.Diagnostics.Metrics, чтобы их мог подобрать OpenTelemetry.
/// </summary>
public sealed class ScrapingMetrics : IScrapingMetrics
{
    public Counter<long> NewsItemsSaved { get; }
    
    public Counter<long> ScrapeJobsCompleted { get; }
    
    public Histogram<double> ScrapeJobDurationMs { get; }
    
    public Histogram<double> RateLimiterWaitDurationMs { get; }
    
    public Histogram<double> ScrapeJobQueueWaitDurationMs { get; }
    
    public ObservableGauge<long> ScrapeJobQueueLength { get; }

    public ScrapingMetrics(IMeterFactory meterFactory, IScrapeJobQueue queue)
    {
        var meter = meterFactory.Create("AiBloger.Scraping", "1.0.0");

        NewsItemsSaved = meter.CreateCounter<long>(
            name: "news_items_saved_total",
            unit: "items",
            description: "Total number of news items persisted to the database");

        ScrapeJobsCompleted = meter.CreateCounter<long>(
            name: "scrape_jobs_completed_total",
            unit: "jobs",
            description: "Total number of successfully completed scrape jobs");

        ScrapeJobDurationMs = meter.CreateHistogram<double>(
            name: "scrape_job_duration_ms",
            unit: "ms",
            description: "End-to-end duration of a scrape job handled by a worker");

        RateLimiterWaitDurationMs = meter.CreateHistogram<double>(
            name: "rate_limiter_wait_duration_ms",
            unit: "ms",
            description: "Time spent waiting for rate limiter permits");

        ScrapeJobQueueWaitDurationMs = meter.CreateHistogram<double>(
            name: "scrape_job_queue_wait_duration_ms",
            unit: "ms",
            description: "How long time scrape job item stay in the queue");

        ScrapeJobQueueLength = meter.CreateObservableGauge<long>(
            name: "scrape_job_queue_length",
            unit: "items",
            description: "Current number of items in the scrape job queue",
            observeValue: () => new Measurement<long>(queue.Count));
    }
}


