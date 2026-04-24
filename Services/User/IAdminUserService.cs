using CinePass_be.DTOs;
using CinePass_be.Models;

namespace CinePass_be.Services;

public interface IAdminUserService
{
    Task<List<AdminUserDto>> GetAllUsersAsync();
    Task<bool> ToggleBanAsync(int userId);
    Task UpdateRoleAsync(int userId, UserRole newRole);
}
