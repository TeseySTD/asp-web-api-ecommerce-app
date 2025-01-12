using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation;
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
        var result = await Result<User>.TryFail()
            .CheckErrorAsync(
                async () => await _context.Users.AnyAsync(u => u.Email == Email.Create(request.Value.Email).Value,
                    cancellationToken),
                new Error("User already exists.", $"User with email: {request.Value.Email} already exists."))
            .CheckErrorAsync(
                async () => await _context.Users.AnyAsync(
                    u => u.PhoneNumber == PhoneNumber.Create(request.Value.PhoneNumber).Value,
                    cancellationToken),
                new Error("User already exists.", $"User with number: {request.Value.PhoneNumber} already exists."))
            .BuildAsync();

        if (result.IsFailure)
            return result;

        var hashedPassword = _passwordHelper.HashPassword(request.Value.Password);

        var user = User.Create(
            name: UserName.Create(request.Value.Name).Value,
            email: Email.Create(request.Value.Email).Value,
            hashedPassword: HashedPassword.Create(hashedPassword).Value,
            phoneNumber: PhoneNumber.Create(request.Value.PhoneNumber).Value,
            role: Enum.Parse<User.UserRole>(request.Value.Role)
        );

        await _context.Users.AddAsync(user, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return user;
    }
}