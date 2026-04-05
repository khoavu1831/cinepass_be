using CinePass_be.DTOs.Movie;
using CinePass_be.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinePass_be.Controllers
{
  [Route("api/movies")]
  [ApiController]
  public class MoviesController : ControllerBase
  {
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
      _movieService = movieService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "releaseDate",
        [FromQuery] string? order = "desc",
        [FromQuery] string? search = null)
    {
      try
      {
        var result = await _movieService.GetAllAsync(page, pageSize, sortBy, order, search);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return BadRequest(new { message = "Lỗi: " + ex.Message });
      }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
      try
      {
        var result = await _movieService.GetByIdAsync(id);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return NotFound(new { message = "Lỗi: " + ex.Message });
      }
    }
  }
}
