using CinePass_be.Data;
using CinePass_be.DTOS;
using CinePass_be.Models;
using Microsoft.EntityFrameworkCore;

namespace CinePass_be.Repositories;

public class UserRepository : IUserRepository
{
  private readonly AppDbContext _db;

  public UserRepository(AppDbContext db)
  {
    _db = db;
  }

  public async Task<List<User>> GetAllAsync()
  {
    return await _db.Users
      .AsNoTracking()
      .ToListAsync();
  }

  public async Task<User?> GetByIdAsync(int id)
  {
    return await _db.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Id == id);
  }

  public async Task<User?> GetByEmailAsync(string email)
  {
    return await _db.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Email == email);
  }

  public async Task<User?> GetByUsernameAsync(string username)
  {
    return await _db.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Username == username);
  }

  public async Task<User?> GetByIdentifierAsync(string identifier)
  {
    return await _db.Users
      .AsNoTracking()
      .FirstOrDefaultAsync(u => u.Email == identifier || u.Username == identifier);
  }

  public async Task<User> CreateUserAsync(User user)
  {
    _db.Users.Add(user);
    await _db.SaveChangesAsync();
    return user;
  }

}

