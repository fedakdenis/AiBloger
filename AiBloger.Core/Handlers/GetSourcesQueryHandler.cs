using AiBloger.Core.Entities;
using AiBloger.Core.Interfaces;
using AiBloger.Core.Mediator;
using AiBloger.Core.Queries;

namespace AiBloger.Core.Handlers;

public class GetSourcesQueryHandler : IRequestHandler<GetSourcesQuery, IReadOnlyList<Source>>
{
    private readonly ISourceRepository _sourceRepository;

    public GetSourcesQueryHandler(ISourceRepository sourceRepository)
    {
        _sourceRepository = sourceRepository;
    }

    public async Task<IReadOnlyList<Source>> Handle(GetSourcesQuery request, CancellationToken cancellationToken)
    {
        var sources = await _sourceRepository.GetAllAsync();
        return sources.ToList().AsReadOnly();
    }
}

