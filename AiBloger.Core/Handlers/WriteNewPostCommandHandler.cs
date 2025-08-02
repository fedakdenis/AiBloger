using AiBloger.Core.Entities;
using AiBloger.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using AiBloger.Core.Commands;

namespace AiBloger.Core.Handlers;

public class WriteNewPostCommandHandler : IRequestHandler<WriteNewPostCommand, PostInfo>
{
    private readonly IAuthorService _openAiService;
    private readonly INewsRepository _newsRepository;
    private readonly IPostRepository _postRepository;
    private readonly IBlogerService _telegramService;
    private readonly ILogger<WriteNewPostCommandHandler> _logger;

    public WriteNewPostCommandHandler(
        IAuthorService openAiService,
        INewsRepository newsRepository,
        IPostRepository postRepository,
        IBlogerService telegramService,
        ILogger<WriteNewPostCommandHandler> logger)
    {
        _openAiService = openAiService;
        _newsRepository = newsRepository;
        _postRepository = postRepository;
        _telegramService = telegramService;
        _logger = logger;
    }


    public async Task<PostInfo> Handle(WriteNewPostCommand request, CancellationToken cancellationToken)
    {
        var latestNews = await _newsRepository.GetLatestNewsAsync(TimeSpan.FromHours(24));
        var latestTitles = latestNews.Select(x => new NewsTitle
        {
            Id = x.Id.ToString(),
            Title = x.Title
        }).ToList();
        var bestTitles = await _openAiService.SelectBestTitlesAsync(latestTitles, 1);
        var theBestTitleId = bestTitles.SelectedIds.First();
        var theBestNew = latestNews.First(x => x.Id == theBestTitleId);
        var newPostInfo = await _openAiService.ProcessUrlAsync(theBestNew.Url);
        var newPost = new Post
        {
            Title = newPostInfo.Title,
            Text = newPostInfo.Post,
            NewsItemId = theBestTitleId,
            NewsItem = theBestNew
        };
        await _postRepository.AddAsync(newPost);
        await _telegramService.PublishPostAsync(newPost);
        return newPostInfo;
    }
}
