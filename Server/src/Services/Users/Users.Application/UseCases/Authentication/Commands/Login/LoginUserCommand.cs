using Shared.Core.CQRS;
using Users.Application.Dto;

namespace Users.Application.UseCases.Authentication.Commands.Login;

public record LoginUserCommand(string Email, string Password): ICommand<TokensDto>;