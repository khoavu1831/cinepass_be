using CinePass_be.Data;
using CinePass_be.Models;
using Microsoft.EntityFrameworkCore;

namespace CinePass_be.Repositories;

public class FollowRepository : IFollowRepository
{
    private readonly AppDbContext _db;

    public FollowRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Follow?> GetAsync(int followerId, int followingId)
    {
        return await _db.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
    }

    public async Task<Follow> CreateAsync(Follow follow)
    {
        _db.Follows.Add(follow);
        await _db.SaveChangesAsync();
        return follow;
    }

    public async Task DeleteAsync(Follow follow)
    {
        _db.Follows.Remove(follow);
        await _db.SaveChangesAsync();
    }

    public async Task<List<int>> GetFollowerIdsAsync(int userId)
    {
        return await _db.Follows
            .Where(f => f.FollowingId == userId)
            .Select(f => f.FollowerId)
            .ToListAsync();
    }

    public async Task<List<int>> GetFollowingIdsAsync(int userId)
    {
        return await _db.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToListAsync();
    }
}
