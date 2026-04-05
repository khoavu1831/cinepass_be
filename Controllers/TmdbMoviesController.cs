using CinePass_be.DTOs.Movie;
using CinePass_be.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinePass_be.Controllers
{
  [Route("api/tmdb")]
  [ApiController]
  public class TmdbMoviesController : ControllerBase
  {
    private readonly IMovieService _movieService;

    public TmdbMoviesController(IMovieService movieService)
    {
      _movieService = movieService;
    }

    [HttpGet("movies/search")]
    public async Task<IActionResult> SearchAsync([FromQuery] string query, [FromQuery] int page = 1)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(query))
          return BadRequest(new { message = "Query không được để trống" });

        var results = await _movieService.SearchAndFetchFromTmdbAsync(query, page);
        return Ok(new { data = results, total = results.Count });
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = "Lỗi: " + ex.Message });
      }
    }
  }
}
