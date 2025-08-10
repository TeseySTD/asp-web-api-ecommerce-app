using Shared.Core.API;
using Users.Application.Dto.User;

namespace Users.API.Http.User.Responses;

public record GetUsersResponse(PaginatedResult<UserReadDto> Users);