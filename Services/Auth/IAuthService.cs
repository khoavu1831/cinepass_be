using CinePass_be.DTOS;

namespace CinePass_be.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RefreshAsync(string refreshToken);
    Task LogoutAsync(int userId, string refreshToken);
}
  