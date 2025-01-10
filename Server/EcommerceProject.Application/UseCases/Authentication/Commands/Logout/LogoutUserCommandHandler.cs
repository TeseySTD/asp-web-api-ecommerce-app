using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Logout;

public class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand>
{
    private readonly IApplicationDbContext _context;

    public LogoutUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var result = Result.TryFail()
            .CheckError(!await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken),
                new Error("User does not exists",
                    $"User with id {request.UserId.Value} does not exists."))
            .Build();
        
        if (result.IsSuccess)
        {
            await _context.RefreshTokens
                .Where(r => r.UserId == request.UserId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        return result;
    }
}