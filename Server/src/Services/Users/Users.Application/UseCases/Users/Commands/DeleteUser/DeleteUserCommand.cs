using Shared.Core.CQRS;

namespace Users.Application.UseCases.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : ICommand;