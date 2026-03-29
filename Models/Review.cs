namespace CinePass_be.Models;

/// <summary>
/// Review model - Core content of the platform
/// User-generated reviews for movies with rating and discussion
/// </summary>
public class Review
{
    // Identity
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MovieId { get; set; }

    // Content
    public string Title { get; set; } = string.Empty;   // Review title
    public string Content { get; set; } = string.Empty; // Review body (long-form)
    public decimal Rating { get; set; }                 // 1-10 score

    // Metadata
    public bool HasSpoiler { get; set; } = false;       // Spoiler warning flag
    public bool IsEdited { get; set; } = false;
    public DateTime? EditedAt { get; set; }

    // Engagement (denormalized for quick display)
    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ReviewEmbedding? ReviewEmbedding { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
