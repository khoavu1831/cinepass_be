namespace CinePass_be.Models;

public class Comment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ReviewId { get; set; }

    public string Content { get; set; } = string.Empty;

    public User User { get; set; } = null!;
    public Review Review { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
