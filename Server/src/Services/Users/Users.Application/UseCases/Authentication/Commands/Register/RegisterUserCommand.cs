using Shared.Core.CQRS;
using Users.Application.Dto;
using Users.Application.Dto.User;

namespace Users.Application.UseCases.Authentication.Commands.Register;

public record RegisterUserCommand(UserWriteDto Value) : ICommand<TokensDto>;