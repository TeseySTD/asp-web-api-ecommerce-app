using Shared.Core.API;
using Shared.Core.CQRS;
using Users.Application.Dto.User;

namespace Users.Application.UseCases.Users.Queries.GetUsers;

public record GetUsersQuery(PaginationRequest PaginationRequest) : IQuery<PaginatedResult<UserReadDto>>;
