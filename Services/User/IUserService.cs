using CinePass_be.Models;

namespace CinePass_be.Services;

public interface IUserService
{
  Task<List<User>> GetAllUsers();
}
