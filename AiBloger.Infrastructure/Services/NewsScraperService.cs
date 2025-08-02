using Microsoft.Extensions.Logging;
using AiBloger.Core.Entities;
using AiBloger.Core.Interfaces;
using System.ServiceModel.Syndication;
using System.Xml;

namespace AiBloger.Infrastructure.Services;

public class NewsScraperService : INewsScraperService
{
    private readonly ILogger<NewsScraperService> _logger;
    private readonly HttpClient _httpClient;

    public NewsScraperService(ILogger<NewsScraperService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<List<NewsItem>> ScrapeNewsAsync(string sourceUrl, DateTime? latestNewsDate)
    {
        _logger.LogInformation("Starting news scraping from: {SourceUrl}", sourceUrl);

        var response = await _httpClient.GetStringAsync(sourceUrl);
        var news = new List<NewsItem>();

        using var stringReader = new StringReader(response);
        using var xmlReader = XmlReader.Create(stringReader);
        var feed = SyndicationFeed.Load(xmlReader);
        var feedItems = feed.Items;
        if (latestNewsDate.HasValue)
        {
            feedItems = feedItems.Where(x => x.PublishDate.UtcDateTime > latestNewsDate);
        }

        foreach (var item in feedItems)
        {
            var title = item.Title?.Text ?? string.Empty;
            
            // Use domain entity directly
            var newsItem = new NewsItem
            {
                Title = title,
                Url = item.Links.FirstOrDefault()?.Uri?.ToString() ?? string.Empty,
                PublishDate = item.PublishDate.UtcDateTime,
                Author = string.Empty,
                Category = string.Empty
            };

            news.Add(newsItem);
        }

        _logger.LogInformation("Scraping completed. Found {Count} news articles", news.Count);
        return news;
    }
}
