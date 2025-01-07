using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Users.Commands.CreateUser;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, User>
{
    private readonly IUsersRepository _usersRepository;

    public CreateUserCommandHandler(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Result<User>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = User.Create(
            name: UserName.Create(request.Value.Name).Value,
            email: Email.Create(request.Value.Email).Value,
            password: Password.Create(request.Value.Password).Value,
            phoneNumber: PhoneNumber.Create(request.Value.PhoneNumber).Value,
            role: Enum.Parse<User.UserRole>(request.Value.Role)
        );
        
        return (await _usersRepository.Add(user, cancellationToken)).Map<Result<User>>(
            onSuccess: () => Result<User>.Success(user),
            onFailure: errors => Result<User>.Failure(errors));
    }
}