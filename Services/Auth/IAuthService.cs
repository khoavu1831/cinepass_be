using CinePass_be.DTOS;

namespace CinePass_be.Services;

public interface IAuthService
{
  Task<AuthResponseDto> ResigterAsync(RegisterRequestDto request);
  Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}
