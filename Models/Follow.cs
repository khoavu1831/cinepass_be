namespace CinePass_be.Models;

/// <summary>
/// Follow model - Social graph for following users
/// One follow per follower/following pair
/// </summary>
public class Follow
{
    // Identity
    public int Id { get; set; }
    public int FollowerId { get; set; }        // User who follows
    public int FollowingId { get; set; }       // User being followed

    // Navigation Properties
    public User Follower { get; set; } = null!;
    public User Following { get; set; } = null!;

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
