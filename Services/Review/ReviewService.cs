using CinePass_be.DTOs;
using CinePass_be.Models;
using CinePass_be.Repositories;

namespace CinePass_be.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IUserRepository _userRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        IMovieRepository movieRepository,
        IUserRepository userRepository)
    {
        _reviewRepository = reviewRepository;
        _movieRepository = movieRepository;
        _userRepository = userRepository;
    }

    public async Task<PaginatedResponseDto<ReviewResponseDto>> GetByMovieIdAsync(
        int movieId, int page = 1, int pageSize = 10, int? currentUserId = null)
    {
        var reviews = await _reviewRepository.GetByMovieIdAsync(movieId, page, pageSize, currentUserId);
        var total = await _reviewRepository.GetCountByMovieIdAsync(movieId);

        var dtos = reviews.Select(r => MapToDto(r, currentUserId)).ToList();

        return new PaginatedResponseDto<ReviewResponseDto>
        {
            Data = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PaginatedResponseDto<ReviewResponseDto>> GetByUserIdAsync(
        int userId, int page = 1, int pageSize = 10)
    {
        var reviews = await _reviewRepository.GetByUserIdAsync(userId, page, pageSize);
        var total = await _reviewRepository.GetCountByUserIdAsync(userId);

        var dtos = reviews.Select(r => MapToDto(r, null)).ToList();

        return new PaginatedResponseDto<ReviewResponseDto>
        {
            Data = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ReviewResponseDto?> GetByIdAsync(int id, int? currentUserId = null)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        return review == null ? null : MapToDto(review, currentUserId);
    }

    public async Task<ReviewResponseDto> CreateAsync(int userId, int movieId, CreateReviewDto dto)
    {
        // Validate
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new Exception("Tiêu đề không được để trống");
        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new Exception("Nội dung không được để trống");
        if (dto.Rating < 1 || dto.Rating > 10)
            throw new Exception("Điểm đánh giá phải từ 1 đến 10");

        // Check movie exists
        var movie = await _movieRepository.GetByIdAsync(movieId)
            ?? throw new Exception($"Không tìm thấy phim có id = {movieId}");

        // Check user exists
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new Exception($"Không tìm thấy người dùng");

        // Check duplicate
        var existing = await _reviewRepository.GetByUserAndMovieAsync(userId, movieId);
        if (existing != null)
            throw new Exception("Bạn đã đánh giá phim này rồi");

        var review = new Review
        {
            UserId = userId,
            MovieId = movieId,
            Title = dto.Title,
            Content = dto.Content,
            Rating = dto.Rating,
            HasSpoiler = dto.HasSpoiler,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _reviewRepository.CreateAsync(review);

        // Update denormalized counters
        user.ReviewCount++;
        await _userRepository.UpdateUserAsync(user);

        // Update movie rating
        await UpdateMovieRatingAsync(movieId);

        // Reload with user info
        var full = await _reviewRepository.GetByIdAsync(created.Id);
        return MapToDto(full!, userId);
    }

    public async Task<ReviewResponseDto?> UpdateAsync(int reviewId, int userId, UpdateReviewDto dto)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null) return null;

        if (review.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền sửa đánh giá này");

        if (!string.IsNullOrWhiteSpace(dto.Title))
            review.Title = dto.Title;

        if (!string.IsNullOrWhiteSpace(dto.Content))
            review.Content = dto.Content;

        if (dto.Rating.HasValue)
        {
            if (dto.Rating < 1 || dto.Rating > 10)
                throw new Exception("Điểm đánh giá phải từ 1 đến 10");
            review.Rating = dto.Rating.Value;
        }

        if (dto.HasSpoiler.HasValue)
            review.HasSpoiler = dto.HasSpoiler.Value;

        review.IsEdited = true;
        review.EditedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;

        var updated = await _reviewRepository.UpdateAsync(review);
        await UpdateMovieRatingAsync(review.MovieId);

        var full = await _reviewRepository.GetByIdAsync(updated.Id);
        return MapToDto(full!, userId);
    }

    public async Task<bool> DeleteAsync(int reviewId, int userId, bool isAdmin = false)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null) return false;

        if (!isAdmin && review.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền xóa đánh giá này");

        var movieId = review.MovieId;
        var authorId = review.UserId;

        var deleted = await _reviewRepository.DeleteAsync(reviewId);
        if (deleted)
        {
            // Update counters
            var author = await _userRepository.GetByIdAsync(authorId);
            if (author != null)
            {
                author.ReviewCount = Math.Max(0, author.ReviewCount - 1);
                await _userRepository.UpdateUserAsync(author);
            }
            await UpdateMovieRatingAsync(movieId);
        }
        return deleted;
    }

    public async Task<bool> ToggleLikeAsync(int userId, int reviewId)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId)
            ?? throw new Exception("Không tìm thấy đánh giá");

        var existingLike = await _reviewRepository.GetLikeAsync(userId, reviewId);

        if (existingLike != null)
        {
            // Unlike
            await _reviewRepository.RemoveLikeAsync(existingLike);
            // Update counter using tracked entity
            var trackedReview = await _reviewRepository.GetByIdAsync(reviewId);
            if (trackedReview != null)
            {
                // Need to update without AsNoTracking
                var reviewEntity = await _reviewRepository.GetByIdAsync(reviewId);
                if (reviewEntity != null)
                {
                    reviewEntity = new Review
                    {
                        Id = reviewEntity.Id,
                        UserId = reviewEntity.UserId,
                        MovieId = reviewEntity.MovieId,
                        Title = reviewEntity.Title,
                        Content = reviewEntity.Content,
                        Rating = reviewEntity.Rating,
                        HasSpoiler = reviewEntity.HasSpoiler,
                        IsEdited = reviewEntity.IsEdited,
                        EditedAt = reviewEntity.EditedAt,
                        LikeCount = Math.Max(0, reviewEntity.LikeCount - 1),
                        CommentCount = reviewEntity.CommentCount,
                        CreatedAt = reviewEntity.CreatedAt,
                        UpdatedAt = reviewEntity.UpdatedAt
                    };
                    await _reviewRepository.UpdateAsync(reviewEntity);
                }
            }
            return false; // unliked
        }
        else
        {
            // Like
            await _reviewRepository.AddLikeAsync(new Like
            {
                UserId = userId,
                ReviewId = reviewId,
                CreatedAt = DateTime.UtcNow
            });

            var reviewEntity = await _reviewRepository.GetByIdAsync(reviewId);
            if (reviewEntity != null)
            {
                var updatedReview = new Review
                {
                    Id = reviewEntity.Id,
                    UserId = reviewEntity.UserId,
                    MovieId = reviewEntity.MovieId,
                    Title = reviewEntity.Title,
                    Content = reviewEntity.Content,
                    Rating = reviewEntity.Rating,
                    HasSpoiler = reviewEntity.HasSpoiler,
                    IsEdited = reviewEntity.IsEdited,
                    EditedAt = reviewEntity.EditedAt,
                    LikeCount = reviewEntity.LikeCount + 1,
                    CommentCount = reviewEntity.CommentCount,
                    CreatedAt = reviewEntity.CreatedAt,
                    UpdatedAt = reviewEntity.UpdatedAt
                };
                await _reviewRepository.UpdateAsync(updatedReview);
            }
            return true; // liked
        }
    }

    public async Task<PaginatedResponseDto<CommentResponseDto>> GetCommentsByReviewIdAsync(
        int reviewId, int page = 1, int pageSize = 20)
    {
        var comments = await _reviewRepository.GetCommentsByReviewIdAsync(reviewId, page, pageSize);
        var total = await _reviewRepository.GetCommentCountByReviewIdAsync(reviewId);

        return new PaginatedResponseDto<CommentResponseDto>
        {
            Data = comments.Select(MapCommentToDto).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CommentResponseDto> AddCommentAsync(int userId, int reviewId, CreateCommentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new Exception("Nội dung bình luận không được để trống");

        var review = await _reviewRepository.GetByIdAsync(reviewId)
            ?? throw new Exception("Không tìm thấy đánh giá");

        var comment = new Comment
        {
            UserId = userId,
            ReviewId = reviewId,
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _reviewRepository.AddCommentAsync(comment);

        // Update review comment counter
        var reviewEntity = await _reviewRepository.GetByIdAsync(reviewId);
        if (reviewEntity != null)
        {
            var updatedReview = new Review
            {
                Id = reviewEntity.Id,
                UserId = reviewEntity.UserId,
                MovieId = reviewEntity.MovieId,
                Title = reviewEntity.Title,
                Content = reviewEntity.Content,
                Rating = reviewEntity.Rating,
                HasSpoiler = reviewEntity.HasSpoiler,
                IsEdited = reviewEntity.IsEdited,
                EditedAt = reviewEntity.EditedAt,
                LikeCount = reviewEntity.LikeCount,
                CommentCount = reviewEntity.CommentCount + 1,
                CreatedAt = reviewEntity.CreatedAt,
                UpdatedAt = reviewEntity.UpdatedAt
            };
            await _reviewRepository.UpdateAsync(updatedReview);
        }

        var full = await _reviewRepository.GetCommentByIdAsync(created.Id);
        return MapCommentToDto(full!);
    }

    public async Task<CommentResponseDto?> UpdateCommentAsync(int commentId, int userId, CreateCommentDto dto)
    {
        var comment = await _reviewRepository.GetCommentByIdAsync(commentId);
        if (comment == null) return null;

        if (comment.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền sửa bình luận này");

        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new Exception("Nội dung bình luận không được để trống");

        comment.Content = dto.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        var updated = await _reviewRepository.UpdateCommentAsync(comment);
        return MapCommentToDto(updated);
    }

    public async Task<bool> DeleteCommentAsync(int commentId, int userId, bool isAdmin = false)
    {
        var comment = await _reviewRepository.GetCommentByIdAsync(commentId);
        if (comment == null) return false;

        if (!isAdmin && comment.UserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền xóa bình luận này");

        var reviewId = comment.ReviewId;
        var deleted = await _reviewRepository.DeleteCommentAsync(commentId);

        if (deleted)
        {
            // Update review comment counter
            var reviewEntity = await _reviewRepository.GetByIdAsync(reviewId);
            if (reviewEntity != null)
            {
                var updatedReview = new Review
                {
                    Id = reviewEntity.Id,
                    UserId = reviewEntity.UserId,
                    MovieId = reviewEntity.MovieId,
                    Title = reviewEntity.Title,
                    Content = reviewEntity.Content,
                    Rating = reviewEntity.Rating,
                    HasSpoiler = reviewEntity.HasSpoiler,
                    IsEdited = reviewEntity.IsEdited,
                    EditedAt = reviewEntity.EditedAt,
                    LikeCount = reviewEntity.LikeCount,
                    CommentCount = Math.Max(0, reviewEntity.CommentCount - 1),
                    CreatedAt = reviewEntity.CreatedAt,
                    UpdatedAt = reviewEntity.UpdatedAt
                };
                await _reviewRepository.UpdateAsync(updatedReview);
            }
        }
        return deleted;
    }

    // ====== Helpers ======

    private async Task UpdateMovieRatingAsync(int movieId)
    {
        var reviews = await _reviewRepository.GetByMovieIdAsync(movieId, 1, int.MaxValue);
        if (reviews.Count == 0) return;

        var movie = await _movieRepository.GetByIdAsync(movieId);
        if (movie == null) return;

        var avgRating = reviews.Average(r => (double)r.Rating);

        var updatedMovie = new Models.Movie
        {
            Id = movie.Id,
            TmdbId = movie.TmdbId,
            Title = movie.Title,
            LocalTitle = movie.LocalTitle,
            Description = movie.Description,
            PosterUrl = movie.PosterUrl,
            BackdropUrl = movie.BackdropUrl,
            TrailerUrl = movie.TrailerUrl,
            Duration = movie.Duration,
            ReleaseDate = movie.ReleaseDate,
            Language = movie.Language,
            Director = movie.Director,
            Cast = movie.Cast,
            GenresJson = movie.GenresJson,
            RatingAvg = (decimal)Math.Round(avgRating, 1),
            ReviewCount = reviews.Count,
            CreatedAt = movie.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        await _movieRepository.UpdateAsync(updatedMovie);
    }

    private static ReviewResponseDto MapToDto(Review review, int? currentUserId)
    {
        return new ReviewResponseDto
        {
            Id = review.Id,
            UserId = review.UserId,
            Username = review.User?.Username ?? string.Empty,
            UserAvatarUrl = review.User?.AvatarUrl,
            MovieId = review.MovieId,
            MovieTitle = review.Movie?.Title ?? string.Empty,
            Title = review.Title,
            Content = review.Content,
            Rating = review.Rating,
            HasSpoiler = review.HasSpoiler,
            IsEdited = review.IsEdited,
            EditedAt = review.EditedAt,
            LikeCount = review.LikeCount,
            CommentCount = review.CommentCount,
            IsLikedByCurrentUser = currentUserId.HasValue && review.Likes.Any(l => l.UserId == currentUserId.Value),
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        };
    }

    private static CommentResponseDto MapCommentToDto(Comment comment)
    {
        return new CommentResponseDto
        {
            Id = comment.Id,
            UserId = comment.UserId,
            Username = comment.User?.Username ?? string.Empty,
            UserAvatarUrl = comment.User?.AvatarUrl,
            ReviewId = comment.ReviewId,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}
