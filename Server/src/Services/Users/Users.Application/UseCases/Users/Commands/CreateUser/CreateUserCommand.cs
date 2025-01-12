using Shared.Core.CQRS;
using Users.Application.Dto.User;
using Users.Core.Models;

namespace Users.Application.UseCases.Users.Commands.CreateUser;

public record CreateUserCommand(UserWriteDto Value) : ICommand<User>;