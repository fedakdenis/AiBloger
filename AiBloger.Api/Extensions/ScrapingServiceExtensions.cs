using AiBloger.Core.Interfaces;
using AiBloger.Infrastructure.Services;

namespace AiBloger.Api.Extensions;

/// <summary>
/// Extension methods for registering continuous scraping services
/// </summary>
public static class ScrapingServiceExtensions
{
    public static IServiceCollection AddContinuousScraping(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Map configuration to individual service options
        services.Configure<ScrapeWorker>(configuration.GetSection("ScrapeWorker"));

        services.Configure<JobScheduler>(configuration.GetSection("JobScheduler"));

        services.Configure<RateLimiter>(configuration.GetSection("RateLimiter"));

        services.Configure<ScrapeQueue>(configuration.GetSection("ScrapeQueue"));
        // Register core services
        services.AddSingleton<IScrapeJobQueue, LifoScrapeJobQueue>();

        services.AddSingleton<IRateLimiterService, RateLimiterService>();

        // Register content scraper with HttpClient
        services.AddHttpClient<IContentScraperService, ContentScraperService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "AiBloger/1.0");
        });

        // Register background services
        services.AddHostedService<ScrapeWorkerService>();
        services.AddHostedService<JobSchedulerService>();

        return services;
    }
}

