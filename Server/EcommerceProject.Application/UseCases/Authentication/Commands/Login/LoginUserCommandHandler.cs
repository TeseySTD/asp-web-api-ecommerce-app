using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Login;

public class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, TokensDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHelper _passwordHelper;
    private readonly ITokenProvider _tokenProvider;

    public LoginUserCommandHandler(ITokenProvider tokenProvider,
        IPasswordHelper passwordHelper, IApplicationDbContext context)
    {
        _tokenProvider = tokenProvider;
        _passwordHelper = passwordHelper;
        _context = context;
    }

    public async Task<Result<TokensDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email).Value;
        User user = default!;

        var result = await Result<TokensDto>.TryFail()
            .CheckError(!await _context.Users.AnyAsync(u => u.Email == email),
                new Error("Incorrect email", $"User with email {request.Email} does not exist"))
            .DropIfFailed()
            .CheckErrorAsync(async () =>
                {
                    user = await _context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email == email, cancellationToken) ?? default!;

                    return !_passwordHelper.VerifyPassword(user!.HashedPassword.Value, request.Password);
                },
                new Error("Incorrect password",
                    $"User with email {request.Email} and password '{request.Password}' does not exist"))
            .BuildAsync();

        if (result.IsFailure)
            return result;

        var refreshToken = _tokenProvider.GenerateRefreshToken(user!);
        var jwtToken = _tokenProvider.GenerateJwtToken(user!);

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new TokensDto(jwtToken, refreshToken.Token);
    }
}