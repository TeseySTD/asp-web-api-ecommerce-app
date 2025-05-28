using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;
using Users.Application.Common.Interfaces;
using Users.Core.Models;
using Users.Core.Models.Entities;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Authentication.Commands.EmailVerification;

public class EmailVerificationCommandHandler(IApplicationDbContext _context) : ICommandHandler<EmailVerificationCommand>
{
    public async Task<Result> Handle(EmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var tokenId = EmailVerificationTokenId.Create(request.Id).Value;
        EmailVerificationToken? token = new();
        User? user = null;

        var result = await Result.Try()
            .Check(!await _context.EmailVerificationTokens.AnyAsync(t => t.Id == tokenId),
                new Error("Email verification token not found", $"Email verification token {tokenId.Value} not found"))
            .DropIfFail()
            .CheckAsync(async () =>
            {
                token = await _context.EmailVerificationTokens.FirstOrDefaultAsync(t => t.Id == tokenId, cancellationToken);
                return Result.Try()
                    .Check(token!.ExpiresOnUtc < DateTime.UtcNow,
                        new Error("Email verification token has expired",
                            $"Email verification token expiration was in {token.ExpiresOnUtc}"))
                    .Build();
            })
            .CheckAsync(async () =>
                {
                    user = await _context.Users.FirstOrDefaultAsync(u => u.Id == token.UserId);
                    return user is null;
                },
                new Error("User not found", $"User with id {token.UserId} not found"))
            .BuildAsync();

        if (result.IsSuccess)
        {
            user!.VerifyEmail();
            _context.EmailVerificationTokens.Remove(token);

            await _context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}