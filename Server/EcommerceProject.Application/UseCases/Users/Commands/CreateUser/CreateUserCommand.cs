using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.User;
using EcommerceProject.Core.Models.Users;

namespace EcommerceProject.Application.UseCases.Users.Commands.CreateUser;

public record CreateUserCommand(UserWriteDto Value) : ICommand<User>;