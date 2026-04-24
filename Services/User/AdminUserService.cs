using CinePass_be.DTOs;
using CinePass_be.Models;
using CinePass_be.Repositories;

namespace CinePass_be.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _userRepository;

    public AdminUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<AdminUserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToList();
    }

    public async Task<bool> ToggleBanAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy user");

        if (user.Role == UserRole.SUPERADMIN)
            throw new InvalidOperationException("Không thể khóa tài khoản SuperAdmin");

        user.IsActive = !user.IsActive;
        await _userRepository.UpdateUserAsync(user);

        return user.IsActive;
    }

    public async Task UpdateRoleAsync(int userId, UserRole newRole)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("Không tìm thấy user");

        if (user.Role == UserRole.SUPERADMIN)
            throw new InvalidOperationException("Không thể thay đổi quyền của SuperAdmin");

        user.Role = newRole;
        await _userRepository.UpdateUserAsync(user);
    }
}
