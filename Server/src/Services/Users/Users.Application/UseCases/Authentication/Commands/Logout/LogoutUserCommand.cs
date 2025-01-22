using Shared.Core.CQRS;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Authentication.Commands.Logout;

public record LogoutUserCommand(Guid UserId) : ICommand;