
using EcommerceProject.Application.Common.Interfaces.Messaging;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Login;

public record LoginUserCommand(string Email, string Password): ICommand<string>;