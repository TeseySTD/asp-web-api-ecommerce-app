using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.User;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Register;

public record RegisterUserCommand(UserWriteDto Value) : ICommand<string>;