using System.Text.Json.Serialization;

namespace AiBloger.Core.Entities;

public class PostInfo
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("post")]
    public string Post { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
