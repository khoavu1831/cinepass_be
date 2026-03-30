using CinePass_be.DTOS;
using CinePass_be.Repositories;

namespace CinePass_be.Services;

public class AuthService : IAuthService
{
  private readonly IUserRepository _userRepository;

  public AuthService(IUserRepository userRepository)
  {
    _userRepository = userRepository;
  }

  public Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
  {
    throw new NotImplementedException();
    
  }

  public Task<AuthResponseDto> ResigterAsync(RegisterRequestDto request)
  {
    throw new NotImplementedException();
  }
}

