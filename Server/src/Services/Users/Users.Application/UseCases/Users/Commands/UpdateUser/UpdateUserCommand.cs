using Shared.Core.CQRS;
using Users.Application.Dto.User;

namespace Users.Application.UseCases.Users.Commands.UpdateUser;

public record UpdateUserCommand(Guid Id, UserUpdateDto Value) : ICommand;