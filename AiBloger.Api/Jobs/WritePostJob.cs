using Quartz;
using AiBloger.Core.Mediator;
using AiBloger.Core.Commands;

namespace AiBloger.Api.Jobs;

public class WritePostJob : IJob
{
    private readonly ILogger<WritePostJob> _logger;
    private readonly IMediator _mediator;

    public WritePostJob(
        ILogger<WritePostJob> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Starting new post creation task");

        try
        {
            var command = new WriteNewPostCommand();
            var postInfo = await _mediator.Send(command, context.CancellationToken);

            _logger.LogInformation("New post successfully created: {PostTitle}", postInfo.Title);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating new post");
        }
    }
}
