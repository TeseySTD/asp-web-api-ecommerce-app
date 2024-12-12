using EcommerceProject.Application.Common.Interfaces.Messaging;

namespace EcommerceProject.Application.UseCases.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid id) : ICommand;