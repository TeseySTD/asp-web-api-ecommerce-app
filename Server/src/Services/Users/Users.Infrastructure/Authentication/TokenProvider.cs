using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Users.Application.Common.Interfaces;
using Users.Core.Models;
using Users.Core.Models.Entities;

namespace Users.Infrastructure.Authentication;

public class TokenProvider(IOptions<JwtSettings> jwtSettings, IOptions<RefreshTokenSettings> refreshTokenSetting)
    : ITokenProvider
{
    public string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new("userId", user.Id.ToString()),
            new("role", user.Role.ToString()),
        };

        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddMinutes(jwtSettings.Value.ExpirationInMinutes),
            claims: claims,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.SecretKey)),
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken GenerateRefreshToken(User user)
    {
        string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var refresh = RefreshToken.Create(token, user.Id,
            DateTime.UtcNow.AddDays(refreshTokenSetting.Value.ExpirationInDays));

        return refresh;
    }
}