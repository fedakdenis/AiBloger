using AiBloger.Core.Commands;
using AiBloger.Core.Entities;
using AiBloger.Core.Mediator;
using AiBloger.Core.Queries;
using Microsoft.AspNetCore.Mvc;

namespace AiBloger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SourcesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SourcesController> _logger;

    public SourcesController(IMediator mediator, ILogger<SourcesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Source>>> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var sources = await _mediator.Send(new GetSourcesQuery(), cancellationToken);
            return Ok(sources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sources");
            return StatusCode(500, "Internal server error while retrieving sources");
        }
    }

    [HttpPost("batch")]
    public async Task<ActionResult<int>> AddBatch([FromBody] IEnumerable<Source> sources, CancellationToken cancellationToken)
    {
        try
        {
            if (sources == null || !sources.Any())
            {
                return BadRequest("Sources list cannot be empty");
            }

            var added = await _mediator.Send(new AddSourcesBatchCommand(sources), cancellationToken);
            return Ok(new { addedCount = added });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding sources batch");
            return StatusCode(500, "Internal server error while adding sources");
        }
    }
}

