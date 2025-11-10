using AiBloger.Core.Entities;
using AiBloger.Core.Interfaces;
using AiBloger.Core.Mediator;
using AiBloger.Core.Queries;

namespace AiBloger.Core.Handlers;

public sealed class GetNewsQueryHandler : IRequestHandler<GetNewsQuery, IReadOnlyList<NewsItem>>
{
    private readonly INewsRepository _newsRepository;

    public GetNewsQueryHandler(INewsRepository newsRepository)
    {
        _newsRepository = newsRepository;
    }

    public async Task<IReadOnlyList<NewsItem>> Handle(GetNewsQuery request, CancellationToken cancellationToken)
    {
        var items = await _newsRepository.GetLatestNewsAsync(request.Interval);
        return items?.ToList() ?? new List<NewsItem>();
    }
}


