using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;
using Users.Application.Common.Interfaces;
using Users.Application.UseCases.Authentication.Commands.RefreshToken;
using Users.Core.Models;
using Users.Core.Models.Entities;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Authentication.Commands.EmailVerification;

public class EmailVerificationCommandHandler(IApplicationDbContext context) : ICommandHandler<EmailVerificationCommand>
{
    public async Task<Result> Handle(EmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var tokenId = EmailVerificationTokenId.Create(request.Id).Value;
        EmailVerificationToken? token = new();
        User? user = null;

        var result = await Result.Try()
            .Check(!await context.EmailVerificationTokens.AnyAsync(t => t.Id == tokenId),
                new TokenNotFoundError(tokenId.Value))
            .DropIfFail()
            .CheckAsync(async () =>
            {
                token = await context.EmailVerificationTokens.FirstOrDefaultAsync(t => t.Id == tokenId,
                    cancellationToken);
                return Result.Try()
                    .Check(token!.ExpiresOnUtc < DateTime.UtcNow,
                        new TokenExpiredError(token.ExpiresOnUtc))
                    .Build();
            })
            .CheckAsync(async () =>
            {
                user = await context.Users.FirstOrDefaultAsync(u => u.Id == token.UserId);
                return user is null
                    ? new TokenUserNotFoundError(token.UserId.Value)
                    : Result.Success();
            })
            .BuildAsync();

        if (result.IsSuccess)
        {
            user!.VerifyEmail();
            context.EmailVerificationTokens.Remove(token);

            await context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public sealed record TokenNotFoundError(Guid TokenId) : Error("Email verification token not found",
        $"Email verification token {TokenId} not found");

    public sealed record TokenExpiredError(DateTime Expires) : Error("Email verification token has expired",
        $"Email verification token expiration was in {Expires}");

    public sealed record TokenUserNotFoundError(Guid TokenId)
        : Error("User not found", $"User with id {TokenId} not found");
}