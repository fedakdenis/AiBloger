using AiBloger.Core.Entities;

namespace AiBloger.Core.Interfaces;

public interface INewsScraperService
{
    Task<List<NewsItem>> ScrapeNewsAsync(string sourceUrl, DateTime? latestNewsDate);
} 
