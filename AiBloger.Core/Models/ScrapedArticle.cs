namespace AiBloger.Core.Models;

/// <summary>
/// Represents scraped article content
/// </summary>
public sealed record ScrapedArticle
{
    public required string Url { get; init; }
    public required string Title { get; init; }
    public string Content { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public DateTime? PublishedDate { get; init; }
    public string Excerpt { get; init; } = string.Empty;
    public string SiteName { get; init; } = string.Empty;
    public TimeSpan ReadingTime { get; init; }
    public DateTime ScrapedAt { get; init; } = DateTime.UtcNow;
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
}

