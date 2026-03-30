using CinePass_be.DTOS;
using CinePass_be.Models;

namespace CinePass_be.Repositories;

public interface IRefreshTokenRepository
{
  Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken);
}
