using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto;
using EcommerceProject.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.RefreshToken;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, TokensDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenProvider _tokenProvider;

    public RefreshTokenCommandHandler(ITokenProvider tokenProvider, IApplicationDbContext context)
    {
        _tokenProvider = tokenProvider;
        _context = context;
    }

    public async Task<Result<TokensDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (!await _context.RefreshTokens.AnyAsync(r => r.Token == request.Token))
            return new Error("Refresh token not found", $"Refresh token {request.Token} not found");

        var refreshToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == request.Token);
        if (refreshToken!.ExpiresOnUtc < DateTime.UtcNow)
            return new Error("Refresh token has expired",
                $"Refresh token expiration was in {refreshToken.ExpiresOnUtc}");

        _context.RefreshTokens.Remove(refreshToken);

        var newRefreshToken = _tokenProvider.GenerateRefreshToken(refreshToken!.User);
        var newAccessToken = _tokenProvider.GenerateJwtToken(refreshToken!.User);
        
        await _context.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new TokensDto(newAccessToken, newRefreshToken.Token);
    }
}