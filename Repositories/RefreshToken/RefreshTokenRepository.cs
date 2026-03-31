using CinePass_be.Data;
using CinePass_be.Models;
using Microsoft.EntityFrameworkCore;

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

  public async Task<RefreshToken?> GetByTokenAsync(string token)
  {
    return await _db.RefreshTokens
      .AsNoTracking()
      .FirstOrDefaultAsync(rt => rt.Token == token);
  }

  public async Task<RefreshToken?> RevokeAsync(int userId, string token)
  {
    var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == token);

    if (refreshToken != null)
    {
      refreshToken.IsRevoked = true;
      _db.RefreshTokens.Update(refreshToken);
      await _db.SaveChangesAsync();
    }

    return refreshToken;
  }

  public async Task<List<RefreshToken>> GetByUserIdAsync(int userId)
  {
    return await _db.RefreshTokens
      .Where(rt => rt.UserId == userId && !rt.IsRevoked)
      .ToListAsync();
  }
}

