using System.Text.Json.Serialization;

namespace AiBloger.Core.Entities;

public class SelectedNews
{
    [JsonPropertyName("selected_ids")]
    public List<int> SelectedIds { get; set; } = new();
}
