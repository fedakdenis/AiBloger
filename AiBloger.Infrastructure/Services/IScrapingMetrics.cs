using System.Diagnostics.Metrics;

namespace AiBloger.Infrastructure.Services;

/// <summary>
/// Интерфейс для метрик скрейпинга
/// </summary>
public interface IScrapingMetrics
{
    /// <summary>
    /// Счётчик сохранённых новостей
    /// </summary>
    Counter<long> NewsItemsSaved { get; }
    
    /// <summary>
    /// Счётчик завершённых задач скрейпинга
    /// </summary>
    Counter<long> ScrapeJobsCompleted { get; }
    
    /// <summary>
    /// Гистограмма длительности выполнения задач скрейпинга
    /// </summary>
    Histogram<double> ScrapeJobDurationMs { get; }
    
    /// <summary>
    /// Гистограмма времени ожидания rate limiter
    /// </summary>
    Histogram<double> RateLimiterWaitDurationMs { get; }
    
    /// <summary>
    /// Гистограмма времени ожидания задачи в очереди
    /// </summary>
    Histogram<double> ScrapeJobQueueWaitDurationMs { get; }
    
    /// <summary>
    /// Текущая длина очереди скрейпинга
    /// </summary>
    ObservableGauge<long> ScrapeJobQueueLength { get; }
}

