using CinePass_be.Models;

namespace CinePass_be.Repositories;

public interface IUserRepository
{
  Task<List<User>> GetAllUsers();
  Task<User?> GetByEmailAsync(string email);
  Task<User?> GetByUsernameAsync(string username);

}
