using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;
using Users.Application.Common.Interfaces;
using Users.Application.Dto.User;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserReadDto>
{
    private readonly IApplicationDbContext _context;
    
    public GetUserByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<UserReadDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync([UserId.Create(request.Id).Value], cancellationToken);
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