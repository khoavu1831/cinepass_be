using CinePass_be.DTOS;
using CinePass_be.Models;
using CinePass_be.Repositories;

namespace CinePass_be.Services;

public class UserService : IUserService
{
  private readonly IUserRepository _userRepository;

  public UserService(IUserRepository userRepository)
  {
    _userRepository = userRepository;
  }

  public async Task<List<User>> GetAllAsync()
  {
    return await _userRepository.GetAllAsync();
  }

  public async Task<User?> GetByEmailAsync(string email)
  {
    return await _userRepository.GetByEmailAsync(email);
  }

  public async Task<User?> GetByUsernameAsync(string username)
  {
    return await _userRepository.GetByUsernameAsync(username);
  }

}
