using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto.User;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserReadDto>
{
    private readonly IUsersRepository _usersRepository;

    public GetUserByIdQueryHandler(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Result<UserReadDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.FindById(UserId.Create(request.Id).Value, cancellationToken);
        if (user == null)
            return Error.NotFound;

        var dto = new UserReadDto(
            Id: user.Id.Value,
            Email: user.Email.Value,
            Name: user.Name.Value,
            Password: user.HashedPassword.Value,
            PhoneNumber: user.PhoneNumber.Value,
            Role: user.Role.ToString()
        );

        return dto;
    }
}