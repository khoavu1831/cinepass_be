using CinePass_be.DTOs;
using CinePass_be.Models;
using CinePass_be.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CinePass_be.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize]
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUserController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        private bool IsAdmin() =>
            User.FindFirst(ClaimTypes.Role)?.Value is string r &&
            (r == UserRole.ADMIN.ToString() || r == UserRole.SUPERADMIN.ToString());

        private bool IsSuperAdmin() =>
            User.FindFirst(ClaimTypes.Role)?.Value == UserRole.SUPERADMIN.ToString();

        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            if (!IsAdmin()) return Forbid();

            var users = await _adminUserService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPut("{id}/ban")]
        public async Task<IActionResult> ToggleBanAsync(int id)
        {
            if (!IsAdmin()) return Forbid();

            var isActive = await _adminUserService.ToggleBanAsync(id);
            return Ok(new { message = isActive ? "Đã mở khóa tài khoản" : "Đã khóa tài khoản" });
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRoleAsync(int id, [FromBody] UpdateUserRoleDto dto)
        {
            if (!IsSuperAdmin()) return Forbid();

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var newRole))
                return BadRequest(new { message = "Quyền không hợp lệ" });

            await _adminUserService.UpdateRoleAsync(id, newRole);
            return Ok(new { message = "Đã cập nhật quyền thành công" });
        }
    }
}
