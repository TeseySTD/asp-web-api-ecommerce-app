using MediatR;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;
using Users.Application.Common.Interfaces;
using Users.Application.Dto;
using Users.Application.UseCases.Users.Commands.CreateUser;

namespace Users.Application.UseCases.Authentication.Commands.Register;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, TokensDto>
{
    private readonly ISender _sender;
    private readonly ITokenProvider _tokenProvider;
    private readonly IApplicationDbContext _context;
    private readonly IEmailHelper _emailHelper;
    private readonly IEmailVerificationLinkFactory _emailVerificationLinkFactory;
    
    public const string EmailSubject = "Email verification for asp ecommerce web api";
    public static string GenerateEmailMessage(string link) => $"To verify your email please click on the link: <a href='{link}'>Verify email</a>";

    public RegisterUserCommandHandler(ISender sender, ITokenProvider tokenProvider, IApplicationDbContext context,
        IEmailHelper emailHelper, IEmailVerificationLinkFactory emailVerificationLinkFactory)
    {
        _sender = sender;
        _tokenProvider = tokenProvider;
        _context = context;
        _emailHelper = emailHelper;
        _emailVerificationLinkFactory = emailVerificationLinkFactory;
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
        var emailVerificationToken = _tokenProvider.GenerateEmailVerificationToken(user);

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.EmailVerificationTokens.AddAsync(emailVerificationToken,cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var emailVerificationLink = _emailVerificationLinkFactory.Create(emailVerificationToken);
        
        if (emailVerificationLink.IsFailure)
            return Result<TokensDto>.Failure(emailVerificationLink.Errors);
        
        await _emailHelper.SendEmailAsync(
            user.Email.Value,
            EmailSubject,
            GenerateEmailMessage(emailVerificationLink.Value)
        );

        return new TokensDto(jwtToken, refreshToken.Token);
    }
}