using CinePass_be.DTOS;
using CinePass_be.Models;

namespace CinePass_be.Services;

public interface IUserService
{
  Task<List<User>> GetAllUsersAsync();
  Task<User?> GetByEmailAsync(string email);
  Task<User?> GetByUsernameAsync(string username);
  // Task<User> CreateUserAsync (CreateUserDto userDto);
}
