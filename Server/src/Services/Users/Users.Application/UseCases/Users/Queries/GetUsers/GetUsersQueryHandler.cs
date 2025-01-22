using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;
using Users.Application.Common.Interfaces;
using Users.Application.Dto.User;

namespace Users.Application.UseCases.Users.Queries.GetUsers;

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