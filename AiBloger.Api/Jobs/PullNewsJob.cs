using Quartz;
using AiBloger.Core.Mediator;
using AiBloger.Core.Commands;
using AiBloger.Core.Queries;

namespace AiBloger.Api.Jobs;

public class PullNewsJob : IJob
{
    private readonly ILogger<PullNewsJob> _logger;
    private readonly IMediator _mediator;

    public PullNewsJob(
        ILogger<PullNewsJob> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
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
