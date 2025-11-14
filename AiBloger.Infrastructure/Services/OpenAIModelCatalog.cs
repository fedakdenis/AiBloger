using AiBloger.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Models;

namespace AiBloger.Infrastructure.Services;

public sealed class OpenAIModelCatalog : IModelCatalog
{
    private readonly OpenAIClient _client;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OpenAIModelCatalog> _logger;

    public OpenAIModelCatalog(string apiKey, IMemoryCache cache, ILogger<OpenAIModelCatalog> logger)
    {
        _client = new OpenAIClient(apiKey);
        _cache = cache;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> GetModelIdsAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "openai_model_ids";
        var result = await _cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
            return CreateModelIdsAsync(cancellationToken);
        });
        return result ?? Array.Empty<string>();
    }

    private async Task<IReadOnlyList<string>> CreateModelIdsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var modelsClient = _client.GetOpenAIModelClient();
            var models = await modelsClient.GetModelsAsync(cancellationToken);
            var ids = models.Value.Select(x => x.Id).ToList();
            ids.Sort(StringComparer.OrdinalIgnoreCase);
            return ids;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch OpenAI models");
            return new[] { "gpt-4.1", "gpt-4o", "gpt-4o-mini" };
        }
    }
}


