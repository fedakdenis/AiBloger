using Quartz;
using AiBloger.Core.Mediator;
using AiBloger.Core.Commands;
using AiBloger.Core.Queries;
using AiBloger.Infrastructure.Services;

namespace AiBloger.Api.Jobs;

public class PullNewsJob : IJob
{
    private readonly ILogger<PullNewsJob> _logger;
    private readonly IMediator _mediator;
    private readonly IScrapingMetrics _metrics;

    public PullNewsJob(
        ILogger<PullNewsJob> logger,
        IMediator mediator,
        IScrapingMetrics metrics)
    {
        _logger = logger;
        _mediator = mediator;
        _metrics = metrics;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting news scraping task");
        var sources = await _mediator.Send(new GetSourcesQuery(), context.CancellationToken);
        // Process news sources
        foreach (var source in sources)
        {
            _logger.LogInformation("Processing source: {SourceName} ({SourceUri})", source.Name, source.Uri);
            try
            {
                var command = new AddNewsFromSourceCommand(source.Name, source.Uri);
                var savedCount = await _mediator.Send(command, context.CancellationToken);

                if (savedCount > 0)
                {
                    _metrics.NewsItemsSaved.Add(savedCount,
                        new KeyValuePair<string, object?>("source_name", source.Name));
                }

                _logger.LogInformation("Processed source {SourceName}: saved {Count} new articles",
                    source.Name, savedCount);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Processing source {SourceName} completed with error", source.Name);
            }

            // Small delay between sources
            await Task.Delay(TimeSpan.FromSeconds(2), context.CancellationToken);
        }
    }
}
