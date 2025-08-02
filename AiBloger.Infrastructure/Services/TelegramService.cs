using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using AiBloger.Core.Entities;
using AiBloger.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AiBloger.Infrastructure.Services;

public class TelegramService : IBlogerService
{
    private readonly ITelegramBotClient _botClient;
    private readonly string _chatId;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(string botToken, string chatId, ILogger<TelegramService> logger)
    {
        _botClient = new TelegramBotClient(botToken);
        _chatId = chatId;
        _logger = logger;
    }

    public async Task<bool> PublishPostAsync(Post post, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = FormatPostMessage(post.Title, post.Text, post.NewsItem.Url);

            var result = await _botClient.SendMessage(
                chatId: _chatId,
                text: message,
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Post '{Title}' successfully published to Telegram. Message ID: {MessageId}", 
                post.Title, result.MessageId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish post '{Title}' to Telegram", post.Title);
            return false;
        }
    }

    private static string FormatPostMessage(string title, string text, string url)
    {
        return new StringBuilder()
            .Append("*")
            .Append(title)
            .AppendLine("*")
            .AppendLine()
            .Append(text)
            .AppendLine()
            .AppendLine(url)
            .ToString();
    }
}