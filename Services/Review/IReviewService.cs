using CinePass_be.DTOs;

namespace CinePass_be.Services;

public interface IReviewService
{
    Task<PaginatedResponseDto<ReviewResponseDto>> GetByMovieIdAsync(int movieId, int page = 1, int pageSize = 10, int? currentUserId = null);
    Task<PaginatedResponseDto<ReviewResponseDto>> GetByUserIdAsync(int userId, int page = 1, int pageSize = 10);
    Task<ReviewResponseDto?> GetByIdAsync(int id, int? currentUserId = null);
    Task<ReviewResponseDto> CreateAsync(int userId, int movieId, CreateReviewDto dto);
    Task<ReviewResponseDto?> UpdateAsync(int reviewId, int userId, UpdateReviewDto dto);
    Task<bool> DeleteAsync(int reviewId, int userId, bool isAdmin = false);

    // Like
    Task<bool> ToggleLikeAsync(int userId, int reviewId);

    // Comment
    Task<PaginatedResponseDto<CommentResponseDto>> GetCommentsByReviewIdAsync(int reviewId, int page = 1, int pageSize = 20);
    Task<CommentResponseDto> AddCommentAsync(int userId, int reviewId, CreateCommentDto dto);
    Task<CommentResponseDto?> UpdateCommentAsync(int commentId, int userId, CreateCommentDto dto);
    Task<bool> DeleteCommentAsync(int commentId, int userId, bool isAdmin = false);
}
