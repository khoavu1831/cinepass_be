using CinePass_be.Data;
using CinePass_be.DTOs;
using CinePass_be.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CinePass_be.Controllers
{
    [Route("api/public/collections")]
    [ApiController]
    public class PublicCollectionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PublicCollectionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetPublicCollections()
        {
            try
            {
                var collections = await _context.Collections
                    .Include(c => c.CollectionMovies)
                        .ThenInclude(cm => cm.Movie)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                var dtos = collections.Select(c => new CollectionDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    Type = c.Type,
                    ThumbnailUrl = c.ThumbnailUrl,
                    DisplayOrder = c.DisplayOrder,
                    Movies = c.CollectionMovies
                        .OrderBy(cm => cm.OrderIndex)
                        .Select(cm => MapToDto(cm.Movie))
                        .ToList()
                });

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi khi lấy danh sách Collection: " + ex.Message });
            }
        }

        private MovieResponseDto MapToDto(Movie movie)
        {
            var genres = new List<string>();

            if (!string.IsNullOrWhiteSpace(movie.GenresJson))
            {
                try
                {
                    genres = System.Text.Json.JsonSerializer.Deserialize<List<string>>(movie.GenresJson) ?? [];
                }
                catch
                {
                    genres = [];
                }
            }

            return new MovieResponseDto
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
                ReleaseDate = movie.ReleaseDate?.ToString("yyyy-MM-dd"),
                Director = movie.Director,
                Cast = movie.Cast,
                Genres = genres,
                RatingAvg = movie.RatingAvg,
                ReviewCount = movie.ReviewCount,
                CreatedAt = movie.CreatedAt,
                UpdatedAt = movie.UpdatedAt
            };
        }
    }
}
