using System.Text.Json.Serialization;

namespace AiBloger.Core.Entities;

public class NewsTitle
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
} 
