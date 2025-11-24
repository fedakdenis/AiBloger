using AiBloger.Core.Entities;
using AiBloger.Core.Enums;

namespace AiBloger.Core.Interfaces;

public interface INewsRepository
{
    Task<DateTime?> GetLatestPublishDateBySourceAsync(string source);
    Task<IEnumerable<NewsItem>> GetLatestNewsAsync(TimeSpan? interval);
    Task<int> AddBatchAsync(IEnumerable<NewsItem> items);
    Task<IReadOnlyList<NewsItem>> GetAndMarkForScrapingAsync(int count, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(int newsItemId, NewsItemStatus status, string? scrapedContent = null, string? errorMessage = null, CancellationToken cancellationToken = default);
}

