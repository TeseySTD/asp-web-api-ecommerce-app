using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;
using Users.Application.Common.Interfaces;
using Users.Application.Dto;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Authentication.Commands.Login;

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

        var result = await Result<TokensDto>.Try()
            .Check(!await _context.Users.AnyAsync(u => u.Email == email),new EmailNotFoundError(email.Value))
            .DropIfFail()
            .CheckAsync(async () =>
                {
                    user = await _context.Users
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.Email == email, cancellationToken) ?? default!;

                    return !_passwordHelper.VerifyPassword(user.HashedPassword.Value, request.Password);
                },
                new IncorrectPasswordError(email.Value, request.Password))
            .BuildAsync();

        if (result.IsFailure)
            return result;

        var refreshToken = _tokenProvider.GenerateRefreshToken(user);
        var jwtToken = _tokenProvider.GenerateJwtToken(user);

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new TokensDto(jwtToken, refreshToken.Token);
    }

    public sealed record EmailNotFoundError(string Email) : Error("Incorrect email.", $"User with email {Email} does not exist.");

    public sealed record IncorrectPasswordError(string Email, string Password) : Error("Incorrect password.",
            $"User with email {Email} and password '{Password}' does not exist.");

}