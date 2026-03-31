using CinePass_be.Models;

namespace CinePass_be.DTOS;
public class CreateUserDto
{
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
}