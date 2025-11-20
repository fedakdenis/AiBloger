namespace AiBloger.Api.Configuration;

public class NewsScraperOptions
{
    public int ScrapingIntervalMinutes { get; set; } = 30;
    public bool EnableLogging { get; set; } = true;
}
