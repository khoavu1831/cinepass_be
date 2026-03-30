using CinePass_be.Data;
using CinePass_be.Models;

namespace CinePass_be.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
  private readonly AppDbContext _db;
  
  public RefreshTokenRepository(AppDbContext db)
  {
    _db = db;
  }

  public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken)
  {
    _db.RefreshTokens.Add(refreshToken);
    await _db.SaveChangesAsync();
    return refreshToken;
  }

}

