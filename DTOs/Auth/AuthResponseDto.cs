namespace CinePass_be.DTOS;

public class AuthResponseDto
{
  public string AccessToken { get; set; } = string.Empty;
  public string RefreshToken { get; set; } = string.Empty;
  public DateTime AccessTokenExpiry { get; set; }
  public int UserId { get; set; }
}