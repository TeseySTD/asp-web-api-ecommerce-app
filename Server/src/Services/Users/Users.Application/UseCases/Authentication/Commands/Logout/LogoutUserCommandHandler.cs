using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation;
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
        var result = Result.TryFail()
            .CheckError(!await _context.Users.AnyAsync(u => u.Id == userId, cancellationToken),
                new Error("User does not exists",
                    $"User with id {request.UserId} does not exists."))
            .Build();
        
        if (result.IsSuccess)
        {
            await _context.RefreshTokens
                .Where(r => r.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        return result;
    }
}