using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Core.Models.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EcommerceProject.Infrastructure.Authentication;

public class JwtTokenProvider(IOptions<JwtSettings> jwtSettings) : IJwtTokenProvider
{
    public string GenerateToken(User user)
    {
        var claims = new List<Claim>
        {
            new ("id", user.Id.Value.ToString()),
            new ("role", user.Role.ToString()),
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
}