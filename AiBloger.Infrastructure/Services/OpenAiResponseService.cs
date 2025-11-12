using OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using AiBloger.Core.Entities;
using AiBloger.Core.Interfaces;

namespace AiBloger.Infrastructure.Services;

public class OpenAiResponseService : IAuthorService
{
    private readonly OpenAIClient _client;
    private readonly string _modelName = "gpt-4.1";
    private string? _cachedSystemMessage;

    public OpenAiResponseService(string apiKey)
    {
        _client = new OpenAIClient(apiKey);
    }

    public async Task<PostInfo> ProcessUrlAsync(string url)
    {
        var systemMessage = await GetSystemMessageFromFileAsync();
        var userMessage = $"Analyze the article at this URL and create a post: {url}";
        var jsonSchema = CreatePostInfoJsonSchema();

        var response = await ExecuteChatRequestAsync(
            systemMessage, 
            userMessage, 
            jsonSchema, 
            "post_info");

        var postInfo = JsonSerializer.Deserialize<PostInfo>(response);
        return postInfo ?? new PostInfo { Title = "Error", Post = "Failed to process article", Url = url };
    }

    public async Task<SelectedNews> SelectBestTitlesAsync(List<NewsTitle> titles, int top)
    {
        var systemMessage = GetSelectTitlesSystemMessage(top);
        var userMessage = CreateSelectTitlesUserMessage(titles);
        var jsonSchema = CreateSelectedNewsJsonSchema();

        var response = await ExecuteChatRequestAsync(
            systemMessage, 
            userMessage, 
            jsonSchema, 
            "selected_news");

        var selectedNews = JsonSerializer.Deserialize<SelectedNews>(response);
        return selectedNews ?? new SelectedNews { SelectedIds = new List<int>() };
    }

    private async Task<string> ExecuteChatRequestAsync(
        string systemMessage, 
        string userMessage, 
        string jsonSchema, 
        string schemaName)
    {
        var chatClient = _client.GetChatClient(_modelName);

        // Add language instruction to system message
        systemMessage = $"{systemMessage}\n\nIMPORTANT: Respond in Russian language.";
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemMessage),
            new UserChatMessage(userMessage)
        };

        var chatOptions = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: schemaName,
                jsonSchema: BinaryData.FromString(jsonSchema),
                jsonSchemaIsStrict: true),
        };

        var response = await chatClient.CompleteChatAsync(messages, chatOptions);
        return response.Value.Content.FirstOrDefault()?.Text ?? "";
    }

    private async Task<string> GetSystemMessageFromFileAsync()
    {
        if (_cachedSystemMessage == null)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "system-message.txt");
            _cachedSystemMessage = await File.ReadAllTextAsync(path);
        }
        return _cachedSystemMessage;
    }

    private static string GetSelectTitlesSystemMessage(int top)
    {
        return string.Format("""
            You are an expert in IT news and editor of a technical channel. Your task is to analyze news headlines and select the most interesting and suitable ones for writing posts.

            Selection criteria:
            1. Relevance and novelty of the topic
            2. Potential interest for IT audience (developers, technical specialists)
            3. Significance of the news for the industry
            4. Ability to write an engaging post without boring technical details
            5. Avoid: routine updates, minor bug fixes, corporate press releases without technical value

            Select the {{Top}} most interesting headlines from the provided list.
            Return the result in JSON format with field: selected_ids (array of strings with Id).
            """, top);
    }

    private static string CreateSelectTitlesUserMessage(List<NewsTitle> titles)
    {
        var titlesText = string.Join("\n", titles.Select((t, i) => $"{i + 1}. [ID: {t.Id}] {t.Title}"));
        return $"Analyze the following news headlines and select 5-10 most interesting ones for IT audience:\n\n{titlesText}";
    }

    private static string CreatePostInfoJsonSchema()
    {
        return JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                title = new { type = "string", description = "Post title" },
                post = new { type = "string", description = "Post text" },
                url = new { type = "string", description = "Original article URL" }
            },
            required = new[] { "title", "post", "url" },
            additionalProperties = false
        });
    }

    private static string CreateSelectedNewsJsonSchema()
    {
        return JsonSerializer.Serialize(new
        {
            type = "object",
            properties = new
            {
                selected_ids = new
                {
                    type = "array",
                    items = new { type = "integer" },
                    description = "Array of selected headline IDs"
                }
            },
            required = new[] { "selected_ids" },
            additionalProperties = false
        });
    }
} 