using SmartReader;
using AiBloger.Core.Interfaces;
using AiBloger.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiBloger.Infrastructure.Services;

/// <summary>
/// Scrapes article content using SmartReader library
/// </summary>
public sealed class ContentScraperService : IContentScraperService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContentScraperService> _logger;

    public ContentScraperService(
        HttpClient httpClient,
        ILogger<ContentScraperService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ScrapedArticle> ScrapeAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Scraping content from {Url}", url);

            // Download HTML content
            var htmlContent = await _httpClient.GetStringAsync(url, cancellationToken);
            
            // Use SmartReader to extract article content
            var reader = new Reader(url, htmlContent);
            var article = await reader.GetArticleAsync();

            if (article is null || !article.IsReadable)
            {
                _logger.LogWarning("Article at {Url} is not readable or parsing failed", url);
                return new ScrapedArticle
                {
                    Url = url,
                    Title = string.Empty,
                    IsSuccess = false,
                    ErrorMessage = "Article is not readable or parsing failed"
                };
            }

            var scrapedArticle = new ScrapedArticle
            {
                Url = url,
                Title = article.Title ?? string.Empty,
                Content = article.Content ?? string.Empty,
                Author = article.Author ?? string.Empty,
                PublishedDate = article.PublicationDate,
                Excerpt = article.Excerpt ?? string.Empty,
                SiteName = article.SiteName ?? string.Empty,
                ReadingTime = article.TimeToRead,
                IsSuccess = true
            };

            _logger.LogInformation(
                "Successfully scraped article from {Url}: '{Title}' ({ContentLength} chars, {ReadingTime} min read)",
                url, 
                scrapedArticle.Title,
                scrapedArticle.Content.Length,
                scrapedArticle.ReadingTime.TotalMinutes);

            return scrapedArticle;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error while scraping {Url}", url);
            return new ScrapedArticle
            {
                Url = url,
                Title = string.Empty,
                IsSuccess = false,
                ErrorMessage = $"HTTP error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while scraping {Url}", url);
            return new ScrapedArticle
            {
                Url = url,
                Title = string.Empty,
                IsSuccess = false,
                ErrorMessage = $"Unexpected error: {ex.Message}"
            };
        }
    }
}

