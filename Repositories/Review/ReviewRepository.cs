using CinePass_be.Data;
using CinePass_be.Models;
using Microsoft.EntityFrameworkCore;

namespace CinePass_be.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _db;

    public ReviewRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Review>> GetByMovieIdAsync(int movieId, int page = 1, int pageSize = 10, int? currentUserId = null)
    {
        return await _db.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Likes)
            .Where(r => r.MovieId == movieId)
            .OrderByDescending(r => r.LikeCount)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountByMovieIdAsync(int movieId)
    {
        return await _db.Reviews.CountAsync(r => r.MovieId == movieId);
    }

    public async Task<List<Review>> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10)
    {
        return await _db.Reviews
            .AsNoTracking()
            .Include(r => r.Movie)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountByUserIdAsync(int userId)
    {
        return await _db.Reviews.CountAsync(r => r.UserId == userId);
    }

    public async Task<Review?> GetByIdAsync(int id)
    {
        return await _db.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Likes)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Review?> GetByUserAndMovieAsync(int userId, int movieId)
    {
        return await _db.Reviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.UserId == userId && r.MovieId == movieId);
    }

    public async Task<Review> CreateAsync(Review review)
    {
        _db.Reviews.Add(review);
        await _db.SaveChangesAsync();
        return review;
    }

    public async Task<Review> UpdateAsync(Review review)
    {
        _db.Reviews.Update(review);
        await _db.SaveChangesAsync();
        return review;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var review = await _db.Reviews.FindAsync(id);
        if (review == null) return false;

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        return true;
    }

    // ====== Likes ======

    public async Task<Like?> GetLikeAsync(int userId, int reviewId)
    {
        return await _db.Likes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ReviewId == reviewId);
    }

    public async Task<Like> AddLikeAsync(Like like)
    {
        _db.Likes.Add(like);
        await _db.SaveChangesAsync();
        return like;
    }

    public async Task RemoveLikeAsync(Like like)
    {
        _db.Likes.Remove(like);
        await _db.SaveChangesAsync();
    }

    // ====== Comments ======

    public async Task<List<Comment>> GetCommentsByReviewIdAsync(int reviewId, int page = 1, int pageSize = 20)
    {
        return await _db.Comments
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.ReviewId == reviewId)
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCommentCountByReviewIdAsync(int reviewId)
    {
        return await _db.Comments.CountAsync(c => c.ReviewId == reviewId);
    }

    public async Task<Comment?> GetCommentByIdAsync(int id)
    {
        return await _db.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Comment> AddCommentAsync(Comment comment)
    {
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment> UpdateCommentAsync(Comment comment)
    {
        _db.Comments.Update(comment);
        await _db.SaveChangesAsync();
        return comment;
    }

    public async Task<bool> DeleteCommentAsync(int id)
    {
        var comment = await _db.Comments.FindAsync(id);
        if (comment == null) return false;

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
