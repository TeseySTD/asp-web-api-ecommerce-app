using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.User;
using EcommerceProject.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, IReadOnlyCollection<UserReadDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyCollection<UserReadDto>>> Handle(GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        if (!_context.Users.Any())
            return new Error("Users not found", "There is no users in the database.");

        var userDtos = await _context.Users.AsNoTracking().Select(u =>
            new UserReadDto(
                u.Id.Value,
                u.Name.Value,
                u.Email.Value,
                u.HashedPassword.Value,
                u.PhoneNumber.Value,
                u.Role.ToString()
            )
        ).ToListAsync();

        return userDtos.ToList().AsReadOnly();
    }
}