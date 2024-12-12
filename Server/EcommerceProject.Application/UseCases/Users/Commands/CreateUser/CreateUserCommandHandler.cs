using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Users.Commands.CreateUser;

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IUsersRepository _usersRepository;

    public CreateUserCommandHandler(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // var user = User.Create(
        //     name: UserName.Create(request.Value.Name),
        //     email: Email.Create(request.Value.Email).Value,
        //     password: Password.Create(request.Value.Password).Value,
        //     phoneNumber: PhoneNumber.Create(request.Value.PhoneNumber).Value,
        //     role: Enum.Parse<User.UserRole>(request.Value.Role)
        // );
        
        throw new NotImplementedException();
    }
}