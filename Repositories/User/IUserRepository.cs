using CinePass_be.DTOS;
using CinePass_be.Models;

namespace CinePass_be.Repositories;

public interface IUserRepository
{
  Task<List<User>> GetAllAsync();
  Task<User?> GetByIdAsync(int id);
  Task<User?> GetByEmailAsync(string email);
  Task<User?> GetByUsernameAsync(string username);
  Task<User?> GetByIdentifierAsync(string identifier);
  Task<User> CreateUserAsync(User user);

}
