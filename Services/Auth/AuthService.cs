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

  public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
  {
    // Valid request
    if (string.IsNullOrWhiteSpace(request.Email))
      throw new Exception("Khong duoc de trong email - Auth Service");
    
    if (string.IsNullOrWhiteSpace(request.Password))
      throw new Exception("Khong duoc de trong mat khau - Auth Service");

    var user = await _userRepository.GetByEmailAsync(request.Email) ??
      throw new Exception("Email hoac mat khau khong dung - Auth Service");

    if (!BC.Verify(request.Password, user.PasswordHash))
      throw new Exception("Email hoac mat khau khong dung - Auth Service");

    if (!user.IsActive)
      throw new Exception("Tai khoan da bi khoa - Auth Service");

    // Generate tokens
    var (accessToken, refreshToken, expires) = TokenHelper.GenerateTokens(user, _config);

    await _refreshTokenRepository.CreateRefreshTokenAsync(refreshToken);

    return new AuthResponseDto
    {
      AccessToken = accessToken,
      AccessTokenExpiry = expires,
      RefreshToken = refreshToken.Token,
      Email = user.Email,
      Username = user.Username,
      UserId = user.Id
    };
  }

  public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
  {
    // Valid request
    if (string.IsNullOrWhiteSpace(request.Username))
      throw new Exception("Khong duoc de trong username - Auth Service");

    if (string.IsNullOrWhiteSpace(request.Password))
      throw new Exception("Khong duoc de trong password - Auth Service");

    if (string.IsNullOrWhiteSpace(request.Email))
      throw new Exception("Khong duoc de trong email - Auth Service");

    var existingUsername = await _userRepository.GetByUsernameAsync(request.Username);
    if (existingUsername != null)
      throw new Exception("Da ton tai username - Auth Service");

    var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
    if (existingEmail != null)
      throw new Exception("Da ton tai email - Auth Service");

    if (request.Password.Length < 6)
      throw new Exception("Mat khau phai >=6 ki tu - Auth Service");

    var emailValid = new EmailAddressAttribute();
    if (!emailValid.IsValid(request.Email))
      throw new Exception("Sai dinh dang email - Auth Service");

    // Hash password
    var hashedPassword = BC.HashPassword(request.Password);

    // Create user
    var user = new User
    {
      Username = request.Username,
      Email = request.Email,
      PasswordHash = hashedPassword,
      IsActive = true,
      CreatedAt = DateTime.Now,
      UpdatedAt = DateTime.Now,
    };

    await _userRepository.CreateUserAsync(user);

    // Create key
    var (accessToken, refreshToken, expires) = TokenHelper.GenerateTokens(user, _config);

    await _refreshTokenRepository.CreateRefreshTokenAsync(refreshToken);

    // Return response
    return new AuthResponseDto
    {
      AccessToken = accessToken,
      AccessTokenExpiry = expires,
      RefreshToken = refreshToken.Token,
      UserId = user.Id,
      Email = user.Email,
      Username = user.Username
    };
  }

  public async Task<AuthResponseDto> RefreshAsync(string refreshToken)
  {
    // Validate request
    if (string.IsNullOrWhiteSpace(refreshToken))
      throw new Exception("Refresh token khong duoc de trong - Auth Service");

    var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken) ??
      throw new Exception("Refresh token khong hop le - Auth Service");

    if (storedRefreshToken.IsRevoked)
      throw new Exception("Refresh token da bi thu hoi - Auth Service");

    if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
      throw new Exception("Refresh token da het han - Auth Service");

    var user = await _userRepository.GetByIdAsync(storedRefreshToken.UserId) ??
      throw new Exception("Khong tim thay user - Auth Service");

    if (!user.IsActive)
      throw new Exception("Tai khoan da bi khoa - Auth Service");

    // Generate new tokens
    var (accessToken, newRefreshToken, expires) = TokenHelper.GenerateTokens(user, _config);

    // Revoke old refresh token
    await _refreshTokenRepository.RevokeAsync(user.Id, refreshToken);

    // Save new refresh token
    await _refreshTokenRepository.CreateRefreshTokenAsync(newRefreshToken);

    return new AuthResponseDto
    {
      AccessToken = accessToken,
      AccessTokenExpiry = expires,
      RefreshToken = newRefreshToken.Token,
      UserId = user.Id,
      Email = user.Email,
      Username = user.Username
    };
  }

  public async Task LogoutAsync(int userId, string refreshToken)
  {
    // Validate request
    if (string.IsNullOrWhiteSpace(refreshToken))
      throw new Exception("Refresh token khong duoc de trong - Auth Service");

    // Revoke the refresh token
    var isRevoked = await _refreshTokenRepository.RevokeAsync(userId, refreshToken);

    if (!isRevoked)
      throw new Exception("Phien dang nhap khong hop le hoac da ket thuc - Auth Service");
  }
}

