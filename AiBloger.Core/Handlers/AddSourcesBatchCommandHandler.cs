using AiBloger.Core.Commands;
using AiBloger.Core.Interfaces;
using AiBloger.Core.Mediator;

namespace AiBloger.Core.Handlers;

public class AddSourcesBatchCommandHandler : IRequestHandler<AddSourcesBatchCommand, int>
{
    private readonly ISourceRepository _sourceRepository;

    public AddSourcesBatchCommandHandler(ISourceRepository sourceRepository)
    {
        _sourceRepository = sourceRepository;
    }

    public async Task<int> Handle(AddSourcesBatchCommand request, CancellationToken cancellationToken)
    {
        return await _sourceRepository.AddBatchAsync(request.Sources);
    }
}

