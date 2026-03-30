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

  public async Task<List<User>> GetAllUsersAsync()
  {
    return await _userRepository.GetAllUsersAsync();
  }

  public async Task<User?> GetByEmailAsync(string email)
  {
    return await _userRepository.GetByEmailAsync(email);
  }

  public async Task<User?> GetByUsernameAsync(string username)
  {
    return await _userRepository.GetByUsernameAsync(username);
  }

  // public async Task<User> CreateUserAsync (CreateUserDto userDto)
  // {
  //   // Valid request
  //   // Create new user
    

  //   // return await _userRepository.CreateUserAsync(user);
  // }

}
