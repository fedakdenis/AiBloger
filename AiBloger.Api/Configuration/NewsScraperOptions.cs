namespace AiBloger.Api.Configuration;

public class NewsScraperOptions
{
    public NewsSource[] Sources { get; set; } = Array.Empty<NewsSource>();
    public int ScrapingIntervalMinutes { get; set; } = 30;
    public bool EnableLogging { get; set; } = true;
}

public class NewsSource
{
    public string Uri { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
