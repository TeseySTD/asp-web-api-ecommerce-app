using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.User;

namespace EcommerceProject.Application.UseCases.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IQuery<UserReadDto>;