using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto.User;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Application.UseCases.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, IReadOnlyCollection<UserReadDto>>
{
    private readonly IUsersRepository _usersRepository;

    public GetUsersQueryHandler(IUsersRepository usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<Result<IReadOnlyCollection<UserReadDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = (await _usersRepository.Get(cancellationToken)).ToList();
        if(!users.Any())
            return new Error("Users not found", "There is no users in the database.");

        var userDtos = users.Select(u =>
            new UserReadDto(
                Id: u.Id.Value,
                Name: u.Name.Value,
                Email: u.Email.Value,
                Password: u.Password.Value,
                PhoneNumber: u.PhoneNumber.Value,
                Role: u.Role.ToString()
            )
        );
        
        return userDtos.ToList().AsReadOnly();
    }
}