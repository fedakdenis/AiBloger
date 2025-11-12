using AiBloger.Core.Entities;
using AiBloger.Core.Interfaces;
using AiBloger.Core.Mediator;
using AiBloger.Core.Commands;

namespace AiBloger.Core.Handlers;

public sealed class GeneratePostPreviewCommandHandler : IRequestHandler<GeneratePostPreviewCommand, PostInfo>
{
    private readonly IAuthorService _authorService;

    public GeneratePostPreviewCommandHandler(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    public async Task<PostInfo> Handle(GeneratePostPreviewCommand request, CancellationToken cancellationToken)
    {
        // Note: Current IAuthorService does not accept model parameter; it's reserved for future use.
        var info = await _authorService.ProcessUrlAsync(request.Url);
        return info;
    }
}


