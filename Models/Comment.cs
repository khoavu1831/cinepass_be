namespace CinePass_be.Models;

/// <summary>
/// Comment model - Discussion on reviews
/// Flat structure (no nested comments for MVP simplicity)
/// </summary>
public class Comment
{
    // Identity
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ReviewId { get; set; }

    // Content
    public string Content { get; set; } = string.Empty;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Review Review { get; set; } = null!;

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
