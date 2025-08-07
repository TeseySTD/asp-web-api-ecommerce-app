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
        var result = await ValidateDataAsync(request, cancellationToken);

        if (result.IsSuccess)
        {
            var token = result.Value;
            var user = token.User;

            user!.VerifyEmail();
            context.EmailVerificationTokens.Remove(token);

            await context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private async Task<Result<EmailVerificationToken>> ValidateDataAsync(EmailVerificationCommand request,
        CancellationToken cancellationToken)
    {
        var tokenId = EmailVerificationTokenId.Create(request.Id).Value;
        EmailVerificationToken? token = null;

        var validationResult = await Result<EmailVerificationToken>.Try()
            .Check(!await context.EmailVerificationTokens.AnyAsync(t => t.Id == tokenId),
                new TokenNotFoundError(tokenId.Value))
            .DropIfFail()
            .CheckAsync(async () =>
            {
                token = await context.EmailVerificationTokens
                        .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == tokenId, cancellationToken);

                if (token!.ExpiresOnUtc < DateTime.UtcNow)
                    return new TokenExpiredError(token.ExpiresOnUtc);
                return Result<EmailVerificationToken>.Success(token);
            })
            .Check(() =>
            {
                var user = token!.User;

                if (user is null)
                    return new TokenUserNotFoundError(token!.UserId.Value);
                return Result<EmailVerificationToken>.Success(token);
            })
            .BuildAsync();

        return validationResult.Map(
            onSuccess: () => token!,
            onFailure: errors => Result<EmailVerificationToken>.Failure(errors)
        );
    }

    public sealed record TokenNotFoundError(Guid TokenId) : Error("Email verification token not found",
        $"Email verification token {TokenId} not found");

    public sealed record TokenExpiredError(DateTime Expires) : Error("Email verification token has expired",
        $"Email verification token expiration was in {Expires}");

    public sealed record TokenUserNotFoundError(Guid UserId)
        : Error("User not found", $"User with id {UserId} not found");
}