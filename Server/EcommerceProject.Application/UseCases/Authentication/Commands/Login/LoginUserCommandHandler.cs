using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Login;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, TokensDto>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHelper _passwordHelper;
    private readonly ITokenProvider _tokenProvider;

    public LoginUserCommandHandler(IUsersRepository usersRepository, ITokenProvider tokenProvider,
        IPasswordHelper passwordHelper)
    {
        _usersRepository = usersRepository;
        _tokenProvider = tokenProvider;
        _passwordHelper = passwordHelper;
    }

    public async Task<Result<TokensDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email).Value;
        User user = default!;

        var result = await Result<TokensDto>.TryFail()
            .CheckError(!await _usersRepository.Exists(email, cancellationToken),
                new Error("Incorrect email", $"User with email {request.Email} does not exist"))
            .DropIfFailed()
            .CheckErrorAsync(async () =>
                {
                    user = await _usersRepository.FindByEmail(email, cancellationToken) ?? default!;
                    return !_passwordHelper.VerifyPassword(user!.HashedPassword.Value, request.Password);
                },
                new Error("Incorrect password",
                    $"User with email {request.Email} and password '{request.Password}' does not exist"))
            .BuildAsync();

        if (result.IsFailure)
            return result;

        var refreshToken = _tokenProvider.GenerateRefreshToken(user!);
        var jwtToken = _tokenProvider.GenerateJwtToken(user!);

        var resultRefresh = await _usersRepository.AddRefreshToken(user!, refreshToken, cancellationToken);

        return resultRefresh.Map<Result<TokensDto>>(
            onSuccess: () => new TokensDto(jwtToken, refreshToken.Token),
            onFailure: errors => Result<TokensDto>.Failure(errors));
    }
}