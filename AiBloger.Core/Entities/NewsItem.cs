using System.ComponentModel.DataAnnotations;

namespace AiBloger.Core.Entities;

public class NewsItem
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Url { get; set; } = string.Empty;
    
    public DateTime PublishDate { get; set; }
    
    [Required]
    public string Source { get; set; } = string.Empty;
    
    public string Author { get; set; } = string.Empty;
    
    public string Category { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property for related posts
    public ICollection<Post> Posts { get; set; } = new List<Post>();
} 
