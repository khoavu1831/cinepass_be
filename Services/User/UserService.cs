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

  public Task<List<User>> GetAllUsers()
  {
    return _userRepository.GetAllUsers();
  }
}
