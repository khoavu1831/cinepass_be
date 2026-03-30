using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using CinePass_be.DTOS;
using CinePass_be.Helper;
using CinePass_be.Models;
using CinePass_be.Repositories;
using BC = BCrypt.Net.BCrypt;

namespace CinePass_be.Services;

public class AuthService : IAuthService
{
  private readonly IUserRepository _userRepository;
  private readonly IConfiguration _config;
  private readonly IRefreshTokenRepository _refreshTokenRepository;

  public AuthService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IConfiguration config)
  {
    _userRepository = userRepository;
    _refreshTokenRepository = refreshTokenRepository;
    _config = config;
  }

  public Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
  {
    throw new NotImplementedException();

  }

  public async Task<AuthResponseDto> ResigterAsync(RegisterRequestDto request)
  {
    // Valid request
    var username = request.Username;
    var email = request.Email;
    var password = request.Password;

    if (string.IsNullOrWhiteSpace(username))
      throw new Exception("Loi: Khong duoc de trong username. Auth Service");

    if (string.IsNullOrWhiteSpace(password))
      throw new Exception("Loi: Khong duoc de trong password. Auth Service");

    if (string.IsNullOrWhiteSpace(email))
      throw new Exception("Loi: Khong duoc de trong email. Auth Service");

    var existingUsername = await _userRepository.GetByUsernameAsync(username);
    if (existingUsername != null)
      throw new Exception("Loi: Da ton tai username. Auth Service");

    var existingEmail = await _userRepository.GetByEmailAsync(email);
    if (existingEmail != null)
      throw new Exception("Loi: Da ton tai email. Auth Service");

    if (password.Length < 6)
      throw new Exception("Loi: Mat khau phai >=6 ki tu. Auth Service");

    var emailValid = new EmailAddressAttribute();
    if (!emailValid.IsValid(email))
      throw new Exception("Loi: Sai dinh dang email. Auth Service");

    // Hash password
    var hashedPassword = BC.HashPassword(password);

    // Create user
    var user = new User
    {
      Username = username,
      Email = email,
      PasswordHash = hashedPassword,
      IsActive = true,
      CreatedAt = DateTime.Now,
      UpdatedAt = DateTime.Now,
    };

    await _userRepository.CreateUserAsync(user);

    // Create key
    var (accessToken, refreshToken, expires) = TokenHelper.GenerateTokens(user, _config);
    
    var refreshTokenEntity = new RefreshToken
    {
      UserId = user.Id,
      Token = refreshToken,
      ExpiryDate = DateTime.UtcNow.AddSeconds(int.Parse(_config["Jwt:RefreshTokenExpiry"]!)),
      CreatedAt = DateTime.UtcNow,
      IsRevoked = false,
    };

    await _refreshTokenRepository.CreateRefreshTokenAsync(refreshTokenEntity); 

    // Return response
    return new AuthResponseDto
    {
      AccessToken = accessToken,
      AccessTokenExpiry = expires,
      RefreshToken = refreshToken,
      UserId = user.Id,
      Email = user.Email,
      Username = user.Username
    };
  }
}

