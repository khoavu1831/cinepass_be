using CinePass_be.DTOs;
using CinePass_be.Models;
using CinePass_be.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CinePass_be.Controllers.Admin
{
    [Route("api/admin/collections")]
    [ApiController]
    [Authorize]
    public class AdminCollectionController : ControllerBase
    {
        private readonly ICollectionService _collectionService;

        public AdminCollectionController(ICollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        private bool IsAdmin() =>
            User.FindFirst(ClaimTypes.Role)?.Value is string r &&
            (r == UserRole.ADMIN.ToString() || r == UserRole.SUPERADMIN.ToString());

        [HttpGet]
        public async Task<IActionResult> GetCollectionsAsync()
        {
            if (!IsAdmin()) return Forbid();

            var result = await _collectionService.GetAllAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCollectionAsync([FromBody] CreateCollectionDto dto)
        {
            if (!IsAdmin()) return Forbid();

            var created = await _collectionService.CreateAsync(dto);
            return Ok(new { message = "Tạo collection thành công", data = created });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCollectionAsync(int id, [FromBody] UpdateCollectionDto dto)
        {
            if (!IsAdmin()) return Forbid();

            var updated = await _collectionService.UpdateAsync(id, dto);
            if (updated == null) return NotFound(new { message = "Không tìm thấy collection" });

            return Ok(new { message = "Cập nhật collection thành công", data = updated });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollectionAsync(int id)
        {
            if (!IsAdmin()) return Forbid();

            var success = await _collectionService.DeleteAsync(id);
            if (!success) return NotFound(new { message = "Không tìm thấy collection" });

            return Ok(new { message = "Xoá collection thành công" });
        }

        [HttpPost("{id}/movies")]
        public async Task<IActionResult> AddMovieAsync(int id, [FromBody] AddMovieToCollectionDto dto)
        {
            if (!IsAdmin()) return Forbid();

            await _collectionService.AddMovieAsync(id, dto.MovieId);
            return Ok(new { message = "Thêm phim vào collection thành công" });
        }

        [HttpDelete("{id}/movies/{movieId}")]
        public async Task<IActionResult> RemoveMovieAsync(int id, int movieId)
        {
            if (!IsAdmin()) return Forbid();

            await _collectionService.RemoveMovieAsync(id, movieId);
            return Ok(new { message = "Xoá phim khỏi collection thành công" });
        }

        [HttpPut("{id}/reorder")]
        public async Task<IActionResult> ReorderMoviesAsync(int id, [FromBody] ReorderCollectionMoviesDto dto)
        {
            if (!IsAdmin()) return Forbid();

            await _collectionService.ReorderMoviesAsync(id, dto.MovieIds);
            return Ok(new { message = "Cập nhật thứ tự thành công" });
        }
    }
}
