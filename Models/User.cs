namespace CinePass_be.Models;

/// <summary>
/// User model for social media review platform
/// Stores user identity, profile, and social connections
/// </summary>
public class User
{
    // Identity
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Profile
    public string? Bio { get; set; }                    // Up to 500 chars
    public string? AvatarUrl { get; set; }              // Cloud storage URL

    // Role & Status
    public UserRole Role { get; set; } = UserRole.USER;
    public bool IsActive { get; set; } = true;

    // Stats (denormalized for quick display)
    public int FollowerCount { get; set; } = 0;         // Cache for query performance
    public int FollowingCount { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;

    // Navigation Properties
    public ICollection<Review> Reviews { get; set; } = [];           // Reviews written
    public ICollection<Comment> Comments { get; set; } = [];         // Comments written
    public ICollection<Like> Likes { get; set; } = [];               // Reviews liked
    public ICollection<Follow> FollowersCollection { get; set; } = [];  // Users following me
    public ICollection<Follow> FollowingCollection { get; set; } = []; // Users I follow
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
public enum UserRole { USER, MODERATOR, ADMIN }
