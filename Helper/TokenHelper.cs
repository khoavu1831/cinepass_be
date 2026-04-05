using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CinePass_be.Models;
using Microsoft.IdentityModel.Tokens;

namespace CinePass_be.Helper;

public static class TokenHelper
{
  public static string GenerateRandomToken()
  {
    var randomNumber = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomNumber);
    return Convert.ToBase64String(randomNumber);
  }

  public static (string AccessToken, RefreshToken RefreshToken, DateTime ExpiresIn) GenerateTokens(User user, IConfiguration config)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? ""));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Email, user.Email),
      new Claim(ClaimTypes.Name, user.Username),
      new Claim(ClaimTypes.Role, user.Role.ToString("G")),  // Force enum name format (ADMIN, not 2)
    };

    var accessToken = new JwtSecurityToken(
      issuer: config["Jwt:Issuer"],
      audience: config["Jwt:Audience"],
      claims: claims,
      expires: DateTime.UtcNow.AddMinutes(int.Parse(config["Jwt:AccessTokenExpiry"]!)),
      signingCredentials: creds
    );

    var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);
    var refreshTokenString = GenerateRandomToken();
    var refreshToken = new RefreshToken
    {
      UserId = user.Id,
      Token = refreshTokenString,
      ExpiryDate = DateTime.UtcNow.AddSeconds(int.Parse(config["Jwt:RefreshTokenExpiry"]!)),
      IsRevoked = false,
      CreatedAt = DateTime.UtcNow
    };
    var expires = accessToken.ValidTo;

    return (accessTokenString, refreshToken, expires);
  }
}