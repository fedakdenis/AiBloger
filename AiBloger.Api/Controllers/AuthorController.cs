using Microsoft.AspNetCore.Mvc;
using AiBloger.Core.Entities;
using AiBloger.Core.Interfaces;
using MediatR;
using AiBloger.Core.Commands;

namespace AiBloger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuthorService _openAiService;
    private readonly ILogger<AuthorController> _logger;

    public AuthorController(IMediator mediator, IAuthorService openAiService, ILogger<AuthorController> logger)
    {
        _mediator = mediator;
        _openAiService = openAiService;
        _logger = logger;
    }

    [HttpPost("new-post")]
    public async Task<ActionResult<PostInfo>> WriteNewPost()
    {
        try
        {
            var postInfo = await _mediator.Send(new WriteNewPostCommand());
            return Ok(postInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new post");
            return StatusCode(500, "Internal server error while creating post");
        }
    }
}
