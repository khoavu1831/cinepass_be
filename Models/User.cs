namespace CinePass_be.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }

    public UserRole Role { get; set; } = UserRole.USER;
    public bool IsActive { get; set; } = true;

    public int FollowerCount { get; set; } = 0;
    public int FollowingCount { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;

    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<Follow> FollowersCollection { get; set; } = [];
    public ICollection<Follow> FollowingCollection { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
