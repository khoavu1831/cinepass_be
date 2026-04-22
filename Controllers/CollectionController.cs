using CinePass_be.Data;
using CinePass_be.DTOs;
using CinePass_be.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CinePass_be.Controllers
{
    [Route("api/admin/collections")]
    [ApiController]
    [Authorize]
    public class CollectionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CollectionController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsUserAdmin()
        {
            var rolesClaim = User.FindFirst(ClaimTypes.Role);
            return rolesClaim != null && (rolesClaim.Value == UserRole.ADMIN.ToString() || rolesClaim.Value == UserRole.SUPERADMIN.ToString());
        }

        private IActionResult ValidateAdminAccess()
        {
            if (!IsUserAdmin()) return Forbid();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetCollections()
        {
            try
            {
                if (ValidateAdminAccess() is ForbidResult) return Forbid();

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
                return BadRequest(new { message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionDto dto)
        {
            try
            {
                if (ValidateAdminAccess() is ForbidResult) return Forbid();

                if (string.IsNullOrWhiteSpace(dto.Title))
                    return BadRequest(new { message = "Title is required" });

                var collection = new Collection
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    Type = string.IsNullOrWhiteSpace(dto.Type) ? "standard_horizontal" : dto.Type,
                    ThumbnailUrl = dto.ThumbnailUrl,
                    DisplayOrder = dto.DisplayOrder
                };

                _context.Collections.Add(collection);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Tạo collection thành công", data = collection });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCollection(int id, [FromBody] UpdateCollectionDto dto)
        {
            try
            {
                if (ValidateAdminAccess() is ForbidResult) return Forbid();

                var collection = await _context.Collections.FindAsync(id);
                if (collection == null) return NotFound(new { message = "Không tìm thấy collection" });

                collection.Title = dto.Title;
                collection.Description = dto.Description;
                collection.Type = string.IsNullOrWhiteSpace(dto.Type) ? "standard_horizontal" : dto.Type;
                collection.ThumbnailUrl = dto.ThumbnailUrl;
                collection.DisplayOrder = dto.DisplayOrder;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật collection thành công", data = collection });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            try
            {
                if (ValidateAdminAccess() is ForbidResult) return Forbid();

                var collection = await _context.Collections.FindAsync(id);
                if (collection == null) return NotFound(new { message = "Không tìm thấy collection" });

                _context.Collections.Remove(collection);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xoá collection thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost("{id}/movies")]
        public async Task<IActionResult> AddMovieToCollection(int id, [FromBody] AddMovieToCollectionDto dto)
        {
            try
            {
                if (ValidateAdminAccess() is ForbidResult) return Forbid();

                var collection = await _context.Collections
                    .Include(c => c.CollectionMovies)
                    .FirstOrDefaultAsync(c => c.Id == id);
                
                if (collection == null) return NotFound(new { message = "Không tìm thấy collection" });

                var movie = await _context.Movies.FindAsync(dto.MovieId);
                if (movie == null) return NotFound(new { message = "Không tìm thấy phim" });

                if (collection.CollectionMovies.Any(cm => cm.MovieId == dto.MovieId))
                    return BadRequest(new { message = "Phim đã tồn tại trong collection" });

                var nextOrder = collection.CollectionMovies.Count > 0 
                    ? collection.CollectionMovies.Max(cm => cm.OrderIndex) + 1 
                    : 0;

                collection.CollectionMovies.Add(new CollectionMovie
                {
                    CollectionId = id,
                    MovieId = dto.MovieId,
                    OrderIndex = nextOrder
                });

                await _context.SaveChangesAsync();

                return Ok(new { message = "Thêm phim vào collection thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi: " + ex.Message });
            }
        }
        
        [HttpDelete("{id}/movies/{movieId}")]
        public async Task<IActionResult> RemoveMovieFromCollection(int id, int movieId)
        {
            try
            {
                if (ValidateAdminAccess() is ForbidResult) return Forbid();

                var cm = await _context.CollectionMovies
                    .FirstOrDefaultAsync(x => x.CollectionId == id && x.MovieId == movieId);
                
                if (cm == null) return NotFound(new { message = "Không tìm thấy phim trong collection" });

                _context.CollectionMovies.Remove(cm);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Xoá phim khỏi collection thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPut("{id}/reorder")]
        public async Task<IActionResult> ReorderCollectionMovies(int id, [FromBody] ReorderCollectionMoviesDto dto)
        {
            try
            {
                if (ValidateAdminAccess() is ForbidResult) return Forbid();

                var collectionMovies = await _context.CollectionMovies
                    .Where(cm => cm.CollectionId == id)
                    .ToListAsync();

                if (!collectionMovies.Any()) return Ok();

                for (int i = 0; i < dto.MovieIds.Count; i++)
                {
                    var cm = collectionMovies.FirstOrDefault(x => x.MovieId == dto.MovieIds[i]);
                    if (cm != null)
                    {
                        cm.OrderIndex = i;
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật thứ tự thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi: " + ex.Message });
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
