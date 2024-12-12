using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.User;
using EcommerceProject.Core.Models.Users;

namespace EcommerceProject.Application.UseCases.Users.Queries.GetUsers;

public record GetUsersQuery : IQuery<IReadOnlyCollection<UserReadDto>>;
