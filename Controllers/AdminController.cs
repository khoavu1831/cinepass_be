using CinePass_be.DTOs;
using CinePass_be.Models;
using CinePass_be.Services;
using CinePass_be.Clients.Tmdb;
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
    private readonly ITmdbClient _tmdbClient;

    public AdminController(IMovieService movieService, ITmdbClient tmdbClient)
    {
      _movieService = movieService;
      _tmdbClient = tmdbClient;
    }

    private bool IsUserAdmin()
    {
      var rolesClaim = User.FindFirst(ClaimTypes.Role);
      var userId = User.FindFirst(ClaimTypes.NameIdentifier);

      return rolesClaim != null && (rolesClaim.Value == UserRole.ADMIN.ToString() || rolesClaim.Value == UserRole.SUPERADMIN.ToString());
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

    [HttpGet("tmdb/search")]
    public async Task<IActionResult> SearchTmdbAsync([FromQuery] string query, [FromQuery] int page = 1)
    {
      try
      {
        if (ValidateAdminAccess() is ForbidResult)
          return Forbid();

        if (string.IsNullOrWhiteSpace(query))
          return BadRequest(new { message = "Query không được để trống" });

        var tmdbResponse = await _tmdbClient.SearchMoviesAsync(query, page);
        if (tmdbResponse?.Results == null)
          return Ok(new { data = new List<object>(), total = 0 });

        var results = tmdbResponse.Results.Select(r => new
        {
          TmdbId = r.Id,
          Title = r.Title,
          Year = !string.IsNullOrEmpty(r.ReleaseDate) && r.ReleaseDate.Length >= 4 ? r.ReleaseDate.Substring(0, 4) : "",
          PosterUrl = !string.IsNullOrEmpty(r.PosterPath) ? $"https://image.tmdb.org/t/p/w500{r.PosterPath}" : null,
          Rating = r.VoteAverage
        });

        return Ok(new { data = results, total = results.Count() });
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
