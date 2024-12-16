using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.User;

namespace EcommerceProject.Application.UseCases.Users.Commands.UpdateUser;

public record UpdateUserCommand(Guid Id, UserUpdateDto Value) : ICommand;