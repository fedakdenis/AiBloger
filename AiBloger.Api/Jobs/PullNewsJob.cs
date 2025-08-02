using Quartz;
using Microsoft.Extensions.Options;
using MediatR;
using AiBloger.Api.Configuration;
using AiBloger.Core.Commands;

namespace AiBloger.Api.Jobs;

public class PullNewsJob : IJob
{
    private readonly ILogger<PullNewsJob> _logger;
    private readonly IMediator _mediator;
    private readonly NewsScraperOptions _options;

    public PullNewsJob(
        ILogger<PullNewsJob> logger,
        IMediator mediator,
        IOptions<NewsScraperOptions> options)
    {
        _logger = logger;
        _mediator = mediator;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting news scraping task");
        // Process news sources
        foreach (var source in _options.Sources)
        {
            _logger.LogInformation("Processing source: {SourceName} ({SourceUri})", source.Name, source.Uri);
            try
            {
                var command = new AddNewsFromSourceCommand(source.Name, source.Uri);
                var savedCount = await _mediator.Send(command, context.CancellationToken);
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
