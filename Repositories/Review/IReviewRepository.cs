using CinePass_be.Models;

namespace CinePass_be.Repositories;

public interface IReviewRepository
{
    Task<List<Review>> GetByMovieIdAsync(int movieId, int page = 1, int pageSize = 10, int? currentUserId = null);
    Task<int> GetCountByMovieIdAsync(int movieId);
    Task<List<Review>> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10);
    Task<int> GetCountByUserIdAsync(int userId);
    Task<Review?> GetByIdAsync(int id);
    Task<Review?> GetByUserAndMovieAsync(int userId, int movieId);
    Task<Review> CreateAsync(Review review);
    Task<Review> UpdateAsync(Review review);
    Task<bool> DeleteAsync(int id);

    // Like
    Task<Like?> GetLikeAsync(int userId, int reviewId);
    Task<Like> AddLikeAsync(Like like);
    Task RemoveLikeAsync(Like like);

    // Comment
    Task<List<Comment>> GetCommentsByReviewIdAsync(int reviewId, int page = 1, int pageSize = 20);
    Task<int> GetCommentCountByReviewIdAsync(int reviewId);
    Task<Comment?> GetCommentByIdAsync(int id);
    Task<Comment> AddCommentAsync(Comment comment);
    Task<Comment> UpdateCommentAsync(Comment comment);
    Task<bool> DeleteCommentAsync(int id);

    Task SaveChangesAsync();
}
