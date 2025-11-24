using AiBloger.Core.Models;

namespace AiBloger.Core.Interfaces;

public interface IContentScraperService
{
    Task<ScrapedArticle> ScrapeAsync(string url, CancellationToken cancellationToken = default);
}

