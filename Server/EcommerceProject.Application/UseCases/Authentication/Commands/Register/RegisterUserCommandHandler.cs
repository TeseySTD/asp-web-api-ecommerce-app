using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto;
using EcommerceProject.Application.UseCases.Users.Commands.CreateUser;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users.Entities;
using MediatR;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Register;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, TokensDto>
{
    private readonly ISender _sender;
    private readonly ITokenProvider _tokenProvider;
    private readonly IUsersRepository _usersRepository;

    public RegisterUserCommandHandler(ISender sender, ITokenProvider tokenProvider, IUsersRepository usersRepository)
    {
        _sender = sender;
        _tokenProvider = tokenProvider;
        _usersRepository = usersRepository;
    }

    public async Task<Result<TokensDto>> Handle(RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        var createUserCommand = new CreateUserCommand(request.Value);
        var userResult = await _sender.Send(createUserCommand);

        if (userResult.IsFailure)
            return Result<TokensDto>.Failure(userResult.Errors);

        var user = userResult.Value;

        var refreshToken = _tokenProvider.GenerateRefreshToken(user);
        var jwtToken = _tokenProvider.GenerateJwtToken(user);
        
        var result = await _usersRepository.AddRefreshToken(user, refreshToken, cancellationToken);

        return result.Map<Result<TokensDto>>(
            onSuccess: () => new TokensDto(jwtToken, refreshToken.Token),
            onFailure: errors => Result<TokensDto>.Failure(errors));
    }
}