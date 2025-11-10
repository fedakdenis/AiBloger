using AiBloger.Core.Queries;
using AiBloger.Core.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace AiBloger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SchedulerController> _logger;

    public SchedulerController(IMediator mediator, ILogger<SchedulerController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("jobs")]
    public async Task<ActionResult<IReadOnlyList<QuartzJobInfo>>> GetJobs(CancellationToken cancellationToken)
    {
        try
        {
            var jobs = await _mediator.Send(new GetQuartzJobsQuery(), cancellationToken);
            return Ok(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Quartz jobs");
            return StatusCode(500, "Internal server error while retrieving Quartz jobs");
        }
    }
}


