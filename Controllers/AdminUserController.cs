using CinePass_be.Data;
using CinePass_be.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CinePass_be.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize]
    public class AdminUserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminUserController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsSuperAdmin()
        {
            var rolesClaim = User.FindFirst(ClaimTypes.Role);
            return rolesClaim != null && rolesClaim.Value == UserRole.SUPERADMIN.ToString();
        }

        private bool IsAdminOrSuperAdmin()
        {
            var rolesClaim = User.FindFirst(ClaimTypes.Role);
            return rolesClaim != null && (rolesClaim.Value == UserRole.SUPERADMIN.ToString() || rolesClaim.Value == UserRole.ADMIN.ToString());
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            if (!IsAdminOrSuperAdmin()) return Forbid();

            var users = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt
                })
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("{id}/ban")]
        public async Task<IActionResult> ToggleBanUserAsync(int id)
        {
            if (!IsAdminOrSuperAdmin()) return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Không tìm thấy user" });

            if (user.Role == UserRole.SUPERADMIN)
                return BadRequest(new { message = "Không thể khóa tài khoản SuperAdmin" });

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { message = user.IsActive ? "Đã mở khóa tài khoản" : "Đã khóa tài khoản" });
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRoleAsync(int id, [FromBody] string roleString)
        {
            if (!IsSuperAdmin()) return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Không tìm thấy user" });

            if (user.Role == UserRole.SUPERADMIN)
                return BadRequest(new { message = "Không thể thay đổi quyền của SuperAdmin" });

            if (Enum.TryParse<UserRole>(roleString, true, out var newRole))
            {
                user.Role = newRole;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Đã cập nhật quyền thành công" });
            }

            return BadRequest(new { message = "Quyền không hợp lệ" });
        }
    }
}
