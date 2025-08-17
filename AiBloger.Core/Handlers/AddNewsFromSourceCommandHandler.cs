using AiBloger.Core.Mediator;
using AiBloger.Core.Interfaces;
using AiBloger.Core.Commands;

namespace AiBloger.Core.Handlers;

public class AddNewsFromSourceCommandHandler : IRequestHandler<AddNewsFromSourceCommand, int>
{
    private readonly INewsScraperService _newsScraperService;
    private readonly INewsRepository _newsRepository;

    public AddNewsFromSourceCommandHandler(INewsScraperService newsScraperService, INewsRepository newsRepository)
    {
        _newsScraperService = newsScraperService;
        _newsRepository = newsRepository;
    }

    public async Task<int> Handle(AddNewsFromSourceCommand request, CancellationToken cancellationToken)
    {
        var latestPublishDate = await _newsRepository.GetLatestPublishDateBySourceAsync(request.Source);
        var latest = await _newsScraperService.ScrapeNewsAsync(request.Url, latestPublishDate);
        latest.ForEach(x => x.Source = request.Source);
        return await _newsRepository.AddBatchAsync(latest);
    }
}
