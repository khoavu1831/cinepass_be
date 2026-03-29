using CinePass_be.Models;

namespace CinePass_be.DTOS.User;

public class UserDto
{
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
}