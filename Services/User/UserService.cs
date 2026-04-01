using CinePass_be.DTOS;
using CinePass_be.Models;
using CinePass_be.Repositories;

namespace CinePass_be.Services;

public class UserService : IUserService
{
  private readonly IUserRepository _userRepository;

  public UserService(IUserRepository userRepository)
  {
    _userRepository = userRepository;
  }

  public async Task<List<User>> GetAllAsync()
  {
    return await _userRepository.GetAllAsync();
  }

  public async Task<UserResponseDto> GetByIdAsync(int id)
  {
    if (string.IsNullOrWhiteSpace(id.ToString()))
      throw new Exception($"Khong duoc de trong Id - User Service");

    var user = await _userRepository.GetByIdAsync(id) ??
      throw new Exception($"Khong tim thay nguoi dung co id = {id} - User Service");

    return new UserResponseDto
    {
      Id = id,
      Username = user.Username,
      Email = user.Email,
      Bio = user.Bio,
      AvatarUrl = user.AvatarUrl,
      Role = user.Role,
      IsActive = user.IsActive,
      FollowerCount = user.FollowerCount,
      FollowingCount = user.FollowingCount,
      ReviewCount = user.ReviewCount,
    };
  }

  public async Task<UserResponseDto> GetByEmailAsync(string email)
  {
    if (string.IsNullOrWhiteSpace(email))
      throw new Exception("Khong duoc de email trong - User Service");

    var user = await _userRepository.GetByEmailAsync(email) ??
      throw new Exception("Khong tim thay user! - User Service");

    return new UserResponseDto
    {
      Id = user.Id,
      Username = user.Username,
      Email = user.Email,
      Bio = user.Bio,
      AvatarUrl = user.AvatarUrl,
      Role = user.Role,
      IsActive = user.IsActive,
      FollowerCount = user.FollowerCount,
      FollowingCount = user.FollowingCount,
      ReviewCount = user.ReviewCount,
    };
  }

  public async Task<UserResponseDto> GetByUsernameAsync(string username)
  {
    if (string.IsNullOrWhiteSpace(username))
      throw new Exception("Khong duoc de username trong - User Service");

    var user = await _userRepository.GetByEmailAsync(username) ??
      throw new Exception("Khong tim thay user! - User Service");

    return new UserResponseDto
    {
      Id = user.Id,
      Username = user.Username,
      Email = user.Email,
      Bio = user.Bio,
      AvatarUrl = user.AvatarUrl,
      Role = user.Role,
      IsActive = user.IsActive,
      FollowerCount = user.FollowerCount,
      FollowingCount = user.FollowingCount,
      ReviewCount = user.ReviewCount,
    };
  }

  public async Task<UserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
  {
    if (id <= 0 || string.IsNullOrEmpty(id.ToString()))
      throw new Exception("Id khong hop le hoac rong - User Service");

    var user = await _userRepository.GetByIdAsync(id) ??
      throw new Exception($"Khong tim thay user co id = {id} - User Service");

    if (!string.IsNullOrWhiteSpace(updateUserDto.Username))
    {
      var existingUsername = await _userRepository.GetByUsernameAsync(updateUserDto.Username);
      if (existingUsername != null)
        throw new Exception("Da ton tai username nay - User Service");

      user.Username = updateUserDto.Username;
    }

    if (!string.IsNullOrWhiteSpace(updateUserDto.AvatarUrl))
    {
      user.AvatarUrl = updateUserDto.AvatarUrl;
    }

    if (!string.IsNullOrWhiteSpace(updateUserDto.Bio))
    {
      if (updateUserDto.Bio.Length >= 50)
        throw new Exception("Tieu su khong duoc vuot qua 50 ki tu - User Service");

      user.Bio = updateUserDto.Bio;
    }

    user.UpdatedAt = updateUserDto.UpdatedAt;

    await _userRepository.UpdateUserAsync(user);

    return new UserResponseDto
    {
      Id = user.Id,
      Username = user.Username,
      Email = user.Email,
      Bio = user.Bio,
      AvatarUrl = user.AvatarUrl,
      Role = user.Role,
      IsActive = user.IsActive,
      FollowerCount = user.FollowerCount,
      FollowingCount = user.FollowingCount,
      ReviewCount = user.ReviewCount,
      UpdatedAt = user.UpdatedAt
    };
  }
}
