using AiBloger.Core.Entities;

namespace AiBloger.Core.Interfaces;

public interface INewsRepository
{
    Task<DateTime?> GetLatestPublishDateBySourceAsync(string source);
    Task<IEnumerable<NewsItem>> GetLatestNewsAsync(TimeSpan? interval);
    Task<int> AddBatchAsync(IEnumerable<NewsItem> items);
}

