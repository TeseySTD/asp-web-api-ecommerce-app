using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Users.Commands.CreateUser;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, User>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHelper _passwordHelper;

    public CreateUserCommandHandler(IUsersRepository usersRepository, IPasswordHelper passwordHelper)
    {
        _usersRepository = usersRepository;
        _passwordHelper = passwordHelper;
    }

    public async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var hashedPassword = _passwordHelper.HashPassword(request.Value.Password);
        
        var user = User.Create(
            name: UserName.Create(request.Value.Name).Value,
            email: Email.Create(request.Value.Email).Value,
            hashedPassword: HashedPassword.Create(hashedPassword).Value,
            phoneNumber: PhoneNumber.Create(request.Value.PhoneNumber).Value,
            role: Enum.Parse<User.UserRole>(request.Value.Role)
        );
        
        return (await _usersRepository.Add(user, cancellationToken)).Map<Result<User>>(
            onSuccess: () => Result<User>.Success(user),
            onFailure: errors => Result<User>.Failure(errors));
    }
}