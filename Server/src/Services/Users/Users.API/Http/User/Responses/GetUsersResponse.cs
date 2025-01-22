using Users.Application.Dto.User;

namespace Users.API.Http.User.Responses;

public record GetUsersResponse(IReadOnlyCollection<UserReadDto> Users);