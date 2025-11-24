namespace AiBloger.Core.Models;

/// <summary>
/// Represents a job to scrape content from a URL
/// </summary>
public sealed record ScrapeJob
{
    public required int SourceId { get; init; }
    public required string SourceName { get; init; }
    public required string Url { get; init; }
    public required string ArticleTitle { get; init; }
    public DateTime EnqueuedAt { get; init; } = DateTime.UtcNow;
    public int RetryCount { get; init; } = 0;
    public int MaxRetries { get; init; } = 3;
}

