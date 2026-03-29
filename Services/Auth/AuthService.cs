using CinePass_be.DTOS.Auth;

namespace CinePass_be;

public class AuthService : IAuthService
{
  public Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
  {
    throw new NotImplementedException();
  }

  public Task<AuthResponseDto> ResigterAsync(RegisterRequestDto request)
  {
    throw new NotImplementedException();
  }
}

