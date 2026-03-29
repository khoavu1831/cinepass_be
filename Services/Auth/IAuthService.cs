using CinePass_be.DTOS.Auth;

namespace CinePass_be;

public interface IAuthService
{
  Task<AuthResponseDto> ResigterAsync(RegisterRequestDto request);
  Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
}
