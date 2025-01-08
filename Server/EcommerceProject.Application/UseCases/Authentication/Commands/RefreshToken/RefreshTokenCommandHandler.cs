using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, TokensDto>
{
    private readonly IUsersRepository _usersRepository;
    private readonly ITokenProvider _tokenProvider;

    public RefreshTokenCommandHandler(IUsersRepository usersRepository, ITokenProvider tokenProvider)
    {
        _usersRepository = usersRepository;
        _tokenProvider = tokenProvider;
    }

    public async Task<Result<TokensDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _usersRepository.GetRefreshToken(request.Token, cancellationToken);
        if (refreshToken == null)
            return new Error("Refresh token not found", $"Refresh token {request.Token} not found");

        if (refreshToken.ExpiresOnUtc < DateTime.UtcNow)
            return new Error("Refresh token has expired",
                $"Refresh token expiration was in {refreshToken.ExpiresOnUtc}");

        await _usersRepository.RemoveRefreshToken(request.Token, cancellationToken);
        var user = await _usersRepository.FindById(refreshToken.UserId, cancellationToken);

        var newRefreshToken = _tokenProvider.GenerateRefreshToken(user!);
        await _usersRepository.AddRefreshToken(user!, newRefreshToken, cancellationToken);

        var newAccessToken = _tokenProvider.GenerateJwtToken(user!);

        return new TokensDto(newAccessToken, newRefreshToken.Token);
    }
}