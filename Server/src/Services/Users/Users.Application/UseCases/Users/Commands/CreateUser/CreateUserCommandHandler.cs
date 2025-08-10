using Microsoft.EntityFrameworkCore;
using Shared.Core.Auth;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;
using Users.Application.Common.Interfaces;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Users.Commands.CreateUser;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, User>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHelper _passwordHelper;

    public CreateUserCommandHandler(IPasswordHelper passwordHelper,
        IApplicationDbContext context)
    {
        _passwordHelper = passwordHelper;
        _context = context;
    }

    public async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Value.Email).Value;
        var number = PhoneNumber.Create(request.Value.PhoneNumber).Value;
        var result = await Result<User>.Try()
            .Check(
                await _context.Users.AnyAsync(u => u.Email == email, cancellationToken),
                new EmailIsTakenError(request.Value.Email))
            .CheckAsync(
                async () => await _context.Users.AnyAsync(u => u.PhoneNumber == number, cancellationToken),
                new PhoneIsTakenError(request.Value.PhoneNumber))
            .BuildAsync();

        if (result.IsFailure)
            return result;

        var hashedPassword = _passwordHelper.HashPassword(request.Value.Password);

        var user = User.Create(
            name: UserName.Create(request.Value.Name).Value,
            email: Email.Create(request.Value.Email).Value,
            hashedPassword: HashedPassword.Create(hashedPassword).Value,
            phoneNumber: PhoneNumber.Create(request.Value.PhoneNumber).Value,
            role: Enum.Parse<UserRole>(request.Value.Role)
        );

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }

    public sealed record EmailIsTakenError(string Email)
        : Error("User already exists.", $"User with email: {Email} already exists.");

    public sealed record PhoneIsTakenError(string Number) : Error("User already exists.",
        $"User with number: {Number} already exists.");

}