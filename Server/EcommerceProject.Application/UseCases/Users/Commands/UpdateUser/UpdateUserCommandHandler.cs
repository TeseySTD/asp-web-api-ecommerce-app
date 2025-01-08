using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHelper _passwordHelper;
    
    public UpdateUserCommandHandler(IUsersRepository usersRepository, IPasswordHelper passwordHelper)
    {
        _usersRepository = usersRepository;
        _passwordHelper = passwordHelper;
    }

    public Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var hashedPassword = _passwordHelper.HashPassword(request.Value.Password);

        var user = User.Create(
            id: UserId.Create(request.Id).Value,
            name: UserName.Create(request.Value.Name).Value,
            email: Email.Create(request.Value.Email).Value,
            hashedPassword: HashedPassword.Create(hashedPassword).Value,
            phoneNumber: PhoneNumber.Create(request.Value.PhoneNumber).Value,
            role: Enum.Parse<User.UserRole>(request.Value.Role)
        );
        
        return _usersRepository.Update(user, cancellationToken);
    }
}