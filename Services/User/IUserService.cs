using CinePass_be.DTOs;
using CinePass_be.Models;

namespace CinePass_be.Services;

public interface IUserService
{
  Task<List<User>> GetAllAsync();
  Task<UserResponseDto> GetByIdAsync(int id);
  Task<UserResponseDto> GetByEmailAsync(string email);
  Task<UserResponseDto> GetByUsernameAsync(string username);
  Task<UpdateUserResponseDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
  Task IncrementFollowCountersAsync(int followerId, int followingId);
  Task DecrementFollowCountersAsync(int followerId, int followingId);
}
