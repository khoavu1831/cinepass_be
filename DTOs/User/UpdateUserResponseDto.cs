using CinePass_be.Models;

namespace CinePass_be.DTOS;

public class UpdateUserResponseDto
{
  public int Id { get; set; }
  public string Username { get; set; } = string.Empty;
  public string? Bio { get; set; }
  public string? AvatarUrl { get; set; }
  public DateTime? UpdatedAt { get; set; }
}