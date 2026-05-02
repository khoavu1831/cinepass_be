using CinePass_be.DTOs;
using CinePass_be.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CinePass_be.Controllers;

[Route("api/movies/{movieId}/reviews")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int id))
            return id;
        return null;
    }

    // GET /api/movies/{movieId}/reviews
    [HttpGet]
    public async Task<IActionResult> GetByMovieId(
        int movieId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _reviewService.GetByMovieIdAsync(movieId, page, pageSize, currentUserId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/movies/{movieId}/reviews
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(int movieId, [FromBody] CreateReviewDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _reviewService.CreateAsync(userId.Value, movieId, dto);
            return CreatedAtAction(nameof(GetByMovieId), new { movieId }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // PUT /api/movies/{movieId}/reviews/{reviewId}
    [Authorize]
    [HttpPut("{reviewId}")]
    public async Task<IActionResult> Update(int movieId, int reviewId, [FromBody] UpdateReviewDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _reviewService.UpdateAsync(reviewId, userId.Value, dto);
            if (result == null) return NotFound(new { message = "Không tìm thấy đánh giá" });

            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE /api/movies/{movieId}/reviews/{reviewId}
    [Authorize]
    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> Delete(int movieId, int reviewId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var isAdmin = User.IsInRole("ADMIN") || User.IsInRole("SUPERADMIN");
            var deleted = await _reviewService.DeleteAsync(reviewId, userId.Value, isAdmin);

            if (!deleted) return NotFound(new { message = "Không tìm thấy đánh giá" });
            return Ok(new { message = "Xóa đánh giá thành công" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/movies/{movieId}/reviews/{reviewId}/like
    [Authorize]
    [HttpPost("{reviewId}/like")]
    public async Task<IActionResult> ToggleLike(int movieId, int reviewId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var isLiked = await _reviewService.ToggleLikeAsync(userId.Value, reviewId);
            return Ok(new { liked = isLiked, message = isLiked ? "Đã thích" : "Đã bỏ thích" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/movies/{movieId}/reviews/{reviewId}/comments
    [HttpGet("{reviewId}/comments")]
    public async Task<IActionResult> GetComments(
        int movieId, int reviewId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _reviewService.GetCommentsByReviewIdAsync(reviewId, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/movies/{movieId}/reviews/{reviewId}/comments
    [Authorize]
    [HttpPost("{reviewId}/comments")]
    public async Task<IActionResult> AddComment(int movieId, int reviewId, [FromBody] CreateCommentDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var result = await _reviewService.AddCommentAsync(userId.Value, reviewId, dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE /api/movies/{movieId}/reviews/{reviewId}/comments/{commentId}
    [Authorize]
    [HttpDelete("{reviewId}/comments/{commentId}")]
    public async Task<IActionResult> DeleteComment(int movieId, int reviewId, int commentId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var isAdmin = User.IsInRole("ADMIN") || User.IsInRole("SUPERADMIN");
            var deleted = await _reviewService.DeleteCommentAsync(commentId, userId.Value, isAdmin);

            if (!deleted) return NotFound(new { message = "Không tìm thấy bình luận" });
            return Ok(new { message = "Xóa bình luận thành công" });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
