using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users.Entities;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Login;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, TokensDto>
{
    private readonly IUsersRepository _usersRepository;
    private readonly ITokenProvider _tokenProvider;

    public LoginUserCommandHandler(IUsersRepository usersRepository, ITokenProvider tokenProvider)
    {
        _usersRepository = usersRepository;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result<TokensDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email).Value;
        var password = Password.Create(request.Password).Value;

        var result = Result<TokensDto>.TryFail()
            .CheckError(!await _usersRepository.Exists(email, cancellationToken),
                new Error("Incorrect email", $"User with email {request.Email} does not exist"))
            .CheckError(!await _usersRepository.CheckPassword(email, password, cancellationToken),
                new Error("Incorrect password",
                    $"User with email {request.Email} and password {request.Password} does not exist"))
            .Build();
        
        if(result.IsFailure)
            return result;
        
        var user = await _usersRepository.FindByEmail(email, cancellationToken);
        
        var refreshToken = _tokenProvider.GenerateRefreshToken(user!);
        var jwtToken = _tokenProvider.GenerateJwtToken(user!);
        
         var resultRefresh = await _usersRepository.AddRefreshToken(user!, refreshToken, cancellationToken);

        return resultRefresh.Map<Result<TokensDto>>(
            onSuccess: () => new TokensDto(jwtToken, refreshToken.Token),
            onFailure: errors => Result<TokensDto>.Failure(errors));
    }
}