using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : ICommand;