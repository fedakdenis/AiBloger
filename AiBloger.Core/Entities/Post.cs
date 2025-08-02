using System.ComponentModel.DataAnnotations;

namespace AiBloger.Core.Entities;

public class Post
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Text { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    // Relationship with news item
    public int NewsItemId { get; set; }
    public NewsItem NewsItem { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
