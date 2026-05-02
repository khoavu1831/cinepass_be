using CinePass_be.DTOs;
using CinePass_be.Models;
using CinePass_be.Repositories;
using CinePass_be.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CinePass_be;

[Route("api/users")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IFollowRepository _followRepository;
    private readonly IReviewService _reviewService;

    public UserController(
        IUserService userService,
        IFollowRepository followRepository,
        IReviewService reviewService)
    {
        _userService = userService;
        _followRepository = followRepository;
        _reviewService = reviewService;
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int id))
            return id;
        return null;
    }

    // GET /api/users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        try
        {
            var result = await _userService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // PUT /api/users/{id}
    [HttpPut("{id}")]
    [Authorize(Policy = "SelfOnly")]
    public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        try
        {
            var result = await _userService.UpdateUserAsync(id, updateUserDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/users/{id}/reviews
    [HttpGet("{id}/reviews")]
    public async Task<IActionResult> GetUserReviews(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _reviewService.GetByUserIdAsync(id, page, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // POST /api/users/{id}/follow
    [Authorize]
    [HttpPost("{id}/follow")]
    public async Task<IActionResult> Follow(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();
            if (currentUserId == id) return BadRequest(new { message = "Không thể tự follow chính mình" });

            var existing = await _followRepository.GetAsync(currentUserId.Value, id);
            if (existing != null)
                return BadRequest(new { message = "Đã follow người dùng này rồi" });

            await _followRepository.CreateAsync(new Follow
            {
                FollowerId = currentUserId.Value,
                FollowingId = id,
                CreatedAt = DateTime.UtcNow
            });

            // Update denormalized counters
            await _userService.IncrementFollowCountersAsync(currentUserId.Value, id);

            return Ok(new { message = "Follow thành công", following = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // DELETE /api/users/{id}/follow
    [Authorize]
    [HttpDelete("{id}/follow")]
    public async Task<IActionResult> Unfollow(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();

            var follow = await _followRepository.GetAsync(currentUserId.Value, id);
            if (follow == null)
                return BadRequest(new { message = "Chưa follow người dùng này" });

            await _followRepository.DeleteAsync(follow);

            // Update denormalized counters
            await _userService.DecrementFollowCountersAsync(currentUserId.Value, id);

            return Ok(new { message = "Unfollow thành công", following = false });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/users/{id}/follow-status  (check if current user follows target)
    [Authorize]
    [HttpGet("{id}/follow-status")]
    public async Task<IActionResult> GetFollowStatus(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null) return Unauthorized();

            var follow = await _followRepository.GetAsync(currentUserId.Value, id);
            return Ok(new { following = follow != null });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
