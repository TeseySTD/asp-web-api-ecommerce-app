using EcommerceProject.Application.Dto.User;

namespace EcommerceProject.API.Http.User.Responses;

public record GetUsersResponse(IReadOnlyCollection<UserReadDto> Users);