using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.User;

namespace EcommerceProject.Application.UseCases.Users.Commands.CreateUser;

public record CreateUserCommand(UserWriteDto Value) : ICommand<Guid>;