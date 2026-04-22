namespace CinePass_be.Models;

public class Like
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ReviewId { get; set; }

    public User User { get; set; } = null!;
    public Review Review { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
