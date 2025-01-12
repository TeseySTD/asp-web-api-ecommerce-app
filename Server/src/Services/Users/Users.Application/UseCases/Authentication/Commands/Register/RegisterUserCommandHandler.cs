using MediatR;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Users.Application.Common.Interfaces;
using Users.Application.Dto;
using Users.Application.UseCases.Users.Commands.CreateUser;

namespace Users.Application.UseCases.Authentication.Commands.Register;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, TokensDto>
{
    private readonly ISender _sender;
    private readonly ITokenProvider _tokenProvider;
    private readonly IApplicationDbContext _context;

    public RegisterUserCommandHandler(ISender sender, ITokenProvider tokenProvider, IApplicationDbContext context)
    {
        _sender = sender;
        _tokenProvider = tokenProvider;
        _context = context;
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
        
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new TokensDto(jwtToken, refreshToken.Token);
    }
}