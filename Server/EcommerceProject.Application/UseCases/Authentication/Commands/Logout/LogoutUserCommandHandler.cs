using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Logout;

public class LogoutUserCommandHandler : ICommandHandler<LogoutUserCommand>
{
    private readonly IUsersRepository _usersRepository;

    public LogoutUserCommandHandler(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Result> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        return await _usersRepository.RemoveUserRefreshTokens(request.UserId, cancellationToken);
    }
}