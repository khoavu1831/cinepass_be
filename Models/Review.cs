namespace CinePass_be.Models;

public class Review
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MovieId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public decimal Rating { get; set; }

    public bool HasSpoiler { get; set; } = false;
    public bool IsEdited { get; set; } = false;
    public DateTime? EditedAt { get; set; }

    public int LikeCount { get; set; } = 0;
    public int CommentCount { get; set; } = 0;

    public User User { get; set; } = null!;
    public Movie Movie { get; set; } = null!;
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ReviewEmbedding? ReviewEmbedding { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
