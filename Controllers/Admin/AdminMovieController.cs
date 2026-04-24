using CinePass_be.DTOs;
using CinePass_be.Models;
using CinePass_be.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CinePass_be.Controllers.Admin
{
    [Route("api/admin/movies")]
    [ApiController]
    [Authorize]
    public class AdminMovieController : ControllerBase
    {
        private readonly IMovieService _movieService;

        public AdminMovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        private bool IsAdmin() =>
            User.FindFirst(ClaimTypes.Role)?.Value is string r &&
            (r == UserRole.ADMIN.ToString() || r == UserRole.SUPERADMIN.ToString());

        [HttpGet("tmdb/search")]
        public async Task<IActionResult> SearchTmdbAsync([FromQuery] string query, [FromQuery] int page = 1)
        {
            if (!IsAdmin()) return Forbid();

            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Query không được để trống" });

            var tmdbResponse = await _movieService.SearchTmdbPreviewAsync(query, page);
            return Ok(tmdbResponse);
        }

        [HttpPost("{tmdbId}/import")]
        public async Task<IActionResult> ImportSingleMovieAsync(int tmdbId)
        {
            if (!IsAdmin()) return Forbid();

            var result = await _movieService.FetchAndSaveFromTmdbAsync(tmdbId);
            if (result == null)
                return BadRequest(new { message = "Không thể import phim từ TMDB" });

            return Ok(result);
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportMovieListAsync([FromBody] AdminFetchMoviesRequestDto request)
        {
            if (!IsAdmin()) return Forbid();

            if (string.IsNullOrWhiteSpace(request.Type))
                return BadRequest(new { message = "Type không được để trống. Chọn: popular, top_rated, upcoming, now_playing, genre" });

            var result = await _movieService.FetchAndSaveMovieListAsync(request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMovieAsync(int id, [FromBody] UpdateMovieDto dto)
        {
            if (!IsAdmin()) return Forbid();

            var result = await _movieService.UpdateMovieAsync(id, dto);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy phim" });

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMovieAsync(int id)
        {
            if (!IsAdmin()) return Forbid();

            var success = await _movieService.DeleteMovieAsync(id);
            if (!success)
                return NotFound(new { message = "Không tìm thấy phim" });

            return Ok(new { message = "Phim đã được xoá thành công" });
        }
    }
}
