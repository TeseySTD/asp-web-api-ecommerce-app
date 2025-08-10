using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;
using Users.Application.Common.Interfaces;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Authentication.Commands.Logout;

public class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand>
{
    private readonly IApplicationDbContext _context;

    public LogoutUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.Create(request.UserId).Value;
        var result = Result.Try()
            .Check(!await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken),
                new UserNotFoundError(request.UserId))
            .Build();

        if (result.IsSuccess)
        {
            await _context.RefreshTokens
                .Where(r => r.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        return result;
    }

    public sealed record UserNotFoundError(Guid UserId) : Error("User does not exist",
            $"User with id {UserId} does not exist.");

}