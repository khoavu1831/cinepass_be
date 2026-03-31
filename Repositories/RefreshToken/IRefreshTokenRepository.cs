using CinePass_be.DTOS;
using CinePass_be.Models;

namespace CinePass_be.Repositories;

public interface IRefreshTokenRepository
{
  Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken);
  Task<RefreshToken?> GetByTokenAsync(string token);
  Task<bool> RevokeAsync(int userId, string token);
  Task<List<RefreshToken>> GetByUserIdAsync(int userId);
}
