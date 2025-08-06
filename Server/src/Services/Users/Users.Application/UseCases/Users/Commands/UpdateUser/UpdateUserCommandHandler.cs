using System.Runtime.InteropServices.JavaScript;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Auth;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;
using Users.Application.Common.Interfaces;
using Users.Application.UseCases.Users.Commands.DeleteUser;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHelper _passwordHelper;

    public UpdateUserCommandHandler(IPasswordHelper passwordHelper,
        IApplicationDbContext context)
    {
        _passwordHelper = passwordHelper;
        _context = context;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var id = UserId.Create(request.Id).Value;
        var email = Email.Create(request.Value.Email).Value;
        var number = PhoneNumber.Create(request.Value.PhoneNumber).Value;
        var result = await Result.Try()
            .Check(
                !await _context.Users.AnyAsync(u => u.Id == id, cancellationToken),
                new UserNotFoundError(request.Id))
            .DropIfFail()
            .CheckAsync(
                async () => await _context.Users.AnyAsync(u => u.Email == email && u.Id != id, cancellationToken),
                new IncorrectEmailError(request.Value.Email)
            )
            .CheckAsync(
                async () => await _context.Users.AnyAsync(u => u.PhoneNumber == number && u.Id != id, cancellationToken),
                new IncorrectPhoneNumberError(request.Value.PhoneNumber)
            )
            .BuildAsync();

        if (result.IsFailure)
            return result;

        var hashedPassword = _passwordHelper.HashPassword(request.Value.Password);

        var userToUpdate = await _context.Users.FindAsync([id], cancellationToken);
        userToUpdate!.Update(
            name: UserName.Create(request.Value.Name).Value,
            email: Email.Create(request.Value.Email).Value,
            hashedPassword: HashedPassword.Create(hashedPassword).Value,
            phoneNumber: PhoneNumber.Create(request.Value.PhoneNumber).Value,
            role: Enum.Parse<UserRole>(request.Value.Role)
        );
        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }

    public sealed record UserNotFoundError(Guid Id) : Error("User does not exist.", $"User with id: {Id} not exists.");

    public sealed record IncorrectEmailError(string Email)
        : Error("Incorrect email.", $"User with email: {Email} already exists.");

    public sealed record IncorrectPhoneNumberError(string Number)
        : Error("Incorrect phone number.", $"User with number: {Number} already exists.");
}