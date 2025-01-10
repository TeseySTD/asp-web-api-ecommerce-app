using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Users.Commands.UpdateUser;

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
        var result = await Result.TryFail()
            .CheckErrorAsync(
                async () => !await _context.Users.AnyAsync(u => u.Id == UserId.Create(request.Id).Value, cancellationToken),
                new Error("User not exists.", $"User with id: {request.Id} not exists."))
            .DropIfFailed()
            .CheckErrorAsync(
                async () => await _context.Users.AnyAsync(
                    u => u.Email == Email.Create(request.Value.Email).Value && u.Id != UserId.Create(request.Id).Value, cancellationToken),
                new Error("Incorrect email.", $"User with email: {request.Value.Email} already exists."))
            .CheckErrorAsync(
                async () => await _context.Users.AnyAsync(
                    u => u.PhoneNumber == PhoneNumber.Create(request.Value.PhoneNumber).Value &&
                         u.Id != UserId.Create(request.Id).Value, cancellationToken),
                new Error("Incorrect phone number.", $"User with number: {request.Value.PhoneNumber} already exists."))
            .BuildAsync();

        if (result.IsFailure)
            return result;
        
        var hashedPassword = _passwordHelper.HashPassword(request.Value.Password);

        var userToUpdate = await _context.Users.FindAsync([UserId.Create(request.Id).Value], cancellationToken);
        userToUpdate!.Update(
            name: UserName.Create(request.Value.Name).Value,
            email: Email.Create(request.Value.Email).Value,
            hashedPassword: HashedPassword.Create(hashedPassword).Value,
            phoneNumber: PhoneNumber.Create(request.Value.PhoneNumber).Value,
            role: Enum.Parse<User.UserRole>(request.Value.Role)
        );
        await _context.SaveChangesAsync(cancellationToken);
        
        return result;
    }
}