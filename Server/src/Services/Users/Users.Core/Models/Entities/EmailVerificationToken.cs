using Shared.Core.Domain.Classes;
using Users.Core.Models.ValueObjects;

namespace Users.Core.Models.Entities;

public class EmailVerificationToken : Entity<EmailVerificationTokenId>
{
    public DateTime ExpiresOnUtc { get; set; }
    public UserId UserId { get; set; } = null!;
    public User User { get; set; } = null!;
    
    public static EmailVerificationToken Create(UserId userId, DateTime expiresOnUtc)
    {
        return new EmailVerificationToken()
        {
            Id = EmailVerificationTokenId.Create(Guid.NewGuid()).Value,
            UserId = userId,
            ExpiresOnUtc = expiresOnUtc
        };
    }
}