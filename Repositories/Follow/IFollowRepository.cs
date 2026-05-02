using CinePass_be.Models;

namespace CinePass_be.Repositories;

public interface IFollowRepository
{
    Task<Follow?> GetAsync(int followerId, int followingId);
    Task<Follow> CreateAsync(Follow follow);
    Task DeleteAsync(Follow follow);
    Task<List<int>> GetFollowerIdsAsync(int userId);
    Task<List<int>> GetFollowingIdsAsync(int userId);
}
