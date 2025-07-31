using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Catalog.Tests.Integration.Common;
public static class TestJwtTokens
{
    public static SecurityKey SecurityKey { get; }
    public static SigningCredentials SigningCredentials { get; }

    static TestJwtTokens()
    {
        var keyBytes = new byte[32];
        RandomNumberGenerator.Create().GetBytes(keyBytes);
        SecurityKey = new SymmetricSecurityKey(keyBytes);
        SigningCredentials = new SigningCredentials(SecurityKey, SecurityAlgorithms.HmacSha256);
    }

    public static string GenerateToken(Dictionary<string, object>? additionalClaims = null)
    {
        var claims = new List<Claim>();
        if (additionalClaims != null)
            claims.AddRange(additionalClaims.Select(kv => new Claim(kv.Key, kv.Value.ToString() ?? "")));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: SigningCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
