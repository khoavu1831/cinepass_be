using CinePass_be.DTOs.Movie;
using CinePass_be.Models;
using CinePass_be.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CinePass_be.Controllers
{
  [Route("api/admin")]
  [ApiController]
  [Authorize]
  public class AdminController : ControllerBase
  {
    private readonly IMovieService _movieService;

    public AdminController(IMovieService movieService)
    {
      _movieService = movieService;
    }

    private bool IsUserAdmin()
    {
      var rolesClaim = User.FindFirst(ClaimTypes.Role);
      var userId = User.FindFirst(ClaimTypes.NameIdentifier);

      return rolesClaim != null && rolesClaim.Value == UserRole.ADMIN.ToString();
    }

    private IActionResult ValidateAdminAccess()
    {
      if (!IsUserAdmin())
      {
        Console.WriteLine("khong phai admin");
        return Forbid();
      }

      return Ok();
    }

    [HttpPost("movies/{tmdbId}/import")]
    public async Task<IActionResult> ImportMovieAsync(int tmdbId)
    {
      try
      {
        if (ValidateAdminAccess() is ForbidResult)
          return Forbid();

        var result = await _movieService.FetchAndSaveFromTmdbAsync(tmdbId);
        if (result == null)
          return BadRequest(new { message = "Không thể import phim từ TMDB" });

        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = "Lỗi: " + ex.Message });
      }
    }

    [HttpPost("movies")]
    public async Task<IActionResult> ImportMoviesAsync([FromBody] AdminFetchMoviesRequestDto request)
    {
      try
      {
        if (ValidateAdminAccess() is ForbidResult)
          return Forbid();

        if (string.IsNullOrWhiteSpace(request.Type))
          return BadRequest(new { message = "Type không được để trống. Chọn: popular, top_rated, upcoming, now_playing, genre" });

        var result = await _movieService.FetchAndSaveMovieListAsync(request);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = "Lỗi: " + ex.Message });
      }
    }

    [HttpPut("movies/{id}")]
    public async Task<IActionResult> UpdateMovieAsync(int id, [FromBody] UpdateMovieDto dto)
    {
      try
      {
        if (ValidateAdminAccess() is ForbidResult)
          return Forbid();

        var result = await _movieService.UpdateMovieAsync(id, dto);
        if (result == null)
          return NotFound(new { message = "Không tìm thấy phim" });

        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = "Lỗi: " + ex.Message });
      }
    }

    [HttpDelete("movies/{id}")]
    public async Task<IActionResult> DeleteMovieAsync(int id)
    {
      try
      {
        if (ValidateAdminAccess() is ForbidResult)
          return Forbid();

        var success = await _movieService.DeleteMovieAsync(id);
        if (!success)
          return NotFound(new { message = "Không tìm thấy phim" });

        return Ok(new { message = "Phim đã được xoá thành công" });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = "Lỗi: " + ex.Message });
      }
    }
  }
}
