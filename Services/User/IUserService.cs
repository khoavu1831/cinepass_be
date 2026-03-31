using CinePass_be.DTOS;
using CinePass_be.Models;

namespace CinePass_be.Services;

public interface IUserService
{
  Task<List<User>> GetAllAsync();
  Task<User?> GetByEmailAsync(string email);
  Task<User?> GetByUsernameAsync(string username);
}
