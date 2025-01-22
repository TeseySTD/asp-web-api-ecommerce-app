using Shared.Core.CQRS;
using Users.Application.Dto.User;

namespace Users.Application.UseCases.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IQuery<UserReadDto>;