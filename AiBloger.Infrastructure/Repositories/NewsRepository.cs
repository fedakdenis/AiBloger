using Microsoft.EntityFrameworkCore;
using AiBloger.Core.Entities;
using AiBloger.Core.Interfaces;
using AiBloger.Infrastructure.Data;

namespace AiBloger.Infrastructure.Repositories;

public class NewsRepository : INewsRepository
{
    private readonly NewsDbContext _context;

    public NewsRepository(NewsDbContext context)
    {
        _context = context;
    }

    public async Task<int> AddBatchAsync(IEnumerable<NewsItem> items)
    {
        var urls = items.Select(x => x.Url).Distinct().ToList();
        var urlDublicates = await _context.NewsItems
            .Where(x => urls.Contains(x.Url))
            .Select(x => x.Url)
            .Distinct()
            .ToHashSetAsync();
        
        var itemsBySources = items
            .Where(x => !urlDublicates.Contains(x.Url))
            .DistinctBy(x => x.Url)
            .GroupBy(
                n => n.Source,
                x => x,
                (source, news) => new { source, news = news.ToList() })
            .ToList();
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var result = 0;
            foreach (var itemsBySource in itemsBySources)
            {
                var newNews = await GetNewNewsAsync(itemsBySource.source, itemsBySource.news);
                await _context.NewsItems.AddRangeAsync(newNews);
                result += newNews.Count;
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<List<NewsItem>> GetNewNewsAsync(string source, List<NewsItem> newsItems)
    {
        var lastNewsPuglishDate = await _context
            .NewsItems
            .Where(x => x.Source == source)
            .MaxAsync(x => (DateTime?)x.PublishDate);

        if (lastNewsPuglishDate.HasValue)
        {
            var lastDateUrls = await _context.NewsItems
                .Where(x => x.Source == source && x.PublishDate == lastNewsPuglishDate.Value)
                .Select(x => x.Url)
                .ToHashSetAsync();
            return newsItems
                .Where(x =>
                    x.PublishDate > lastNewsPuglishDate.Value ||
                    x.PublishDate == lastNewsPuglishDate.Value && !lastDateUrls.Contains(x.Url))
                .ToList();
        }

        return newsItems;
    }

    public async Task<DateTime?> GetLatestPublishDateBySourceAsync(string source)
    {
        return await _context.NewsItems
            .Where(n => n.Source == source)
            .MaxAsync(n => (DateTime?)n.PublishDate);
    }

    public async Task<IEnumerable<NewsItem>> GetLatestNewsAsync(TimeSpan? interval)
    {
        IQueryable<NewsItem> result = _context.NewsItems;
        if (interval.HasValue)
        {
            var minPublishDate = DateTime.UtcNow - interval.Value;
            result = result.Where(x => x.PublishDate >= minPublishDate);
        }

        result = result.Where(x => !x.Posts.Any());
        return await result.ToListAsync();
    }
}
