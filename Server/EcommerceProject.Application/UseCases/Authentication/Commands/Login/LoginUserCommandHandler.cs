using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Login;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, string>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IJwtTokenProvider _jwtTokenProvider;

    public LoginUserCommandHandler(IUsersRepository usersRepository, IJwtTokenProvider jwtTokenProvider)
    {
        _usersRepository = usersRepository;
        _jwtTokenProvider = jwtTokenProvider;
    }

    public async Task<Result<string>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email).Value;
        var password = Password.Create(request.Password).Value;

        var result = Result<string>.TryFail()
            .CheckError(!await _usersRepository.Exists(email, cancellationToken),
                new Error("Incorrect email", $"User with email {request.Email} does not exist"))
            .CheckError(!await _usersRepository.CheckPassword(email, password, cancellationToken),
                new Error("Incorrect password",
                    $"User with email {request.Email} and password {request.Password} does not exist"))
            .Build();
        
        if(result.IsFailure)
            return result;
        
        var user = await _usersRepository.FindByEmail(email, cancellationToken);
        return _jwtTokenProvider.GenerateToken(user!);
    }
}