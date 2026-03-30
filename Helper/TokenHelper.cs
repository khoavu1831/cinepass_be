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

  public static (string AccessToken, string RefreshToken, DateTime ExpiresIn)
  GenerateTokens(User user, IConfiguration config)
  {
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? ""));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Email, user.Email),
      new Claim(ClaimTypes.Name, user.Username),
      new Claim(ClaimTypes.Role, user.Role.ToString()),
    };

    var accessToken = new JwtSecurityToken(
      issuer: config["Jwt:Issuer"],
      audience: config["Jwt:Audience"],
      claims: claims,
      expires: DateTime.UtcNow.AddSeconds(int.Parse(config["Jwt:ExpiresMinutes"]!)),
      signingCredentials: creds
    );

    var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);
    var refreshToken = GenerateRandomToken();
    var expires = accessToken.ValidTo;

    return (accessTokenString, refreshToken, expires);
  }
}