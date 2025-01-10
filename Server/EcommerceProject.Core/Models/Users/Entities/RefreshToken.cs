using EcommerceProject.Core.Common.Abstractions.Classes;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Core.Models.Users.Entities;

public class RefreshToken : Entity<RefreshTokenId>
{
    public string Token { get; set; } = null!;
    public DateTime ExpiresOnUtc { get; set; }
    public UserId UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public static RefreshToken Create(string token, UserId userId, DateTime expiresOnUtc)
    {
        return new RefreshToken()
        {
            Id = RefreshTokenId.Create(Guid.NewGuid()).Value,
            Token = token,
            UserId = userId,
            ExpiresOnUtc = expiresOnUtc
        };
    }
}