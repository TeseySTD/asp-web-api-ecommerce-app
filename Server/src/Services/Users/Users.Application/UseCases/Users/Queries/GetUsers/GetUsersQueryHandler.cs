using Microsoft.EntityFrameworkCore;
using Shared.Core.API;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;
using Users.Application.Common.Interfaces;
using Users.Application.Dto.User;

namespace Users.Application.UseCases.Users.Queries.GetUsers;

public class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, PaginatedResult<UserReadDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<UserReadDto>>> Handle(GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var pageIndex = request.PaginationRequest.PageIndex;
        var pageSize = request.PaginationRequest.PageSize;
        
        var userDtos = await _context.Users
            .AsNoTracking()
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .Select(u =>
                new UserReadDto(
                    u.Id.Value,
                    u.Name.Value,
                    u.Email.Value,
                    u.HashedPassword.Value,
                    u.PhoneNumber.Value,
                    u.Role.ToString()
                )
            )
            .ToListAsync();

        if (!userDtos.Any())
            return new UserNotFoundError();
        
        return new PaginatedResult<UserReadDto>(pageIndex, pageSize, userDtos);
    }

    public sealed record UserNotFoundError() : Error("Users not found", "There is no users in the database.");

}