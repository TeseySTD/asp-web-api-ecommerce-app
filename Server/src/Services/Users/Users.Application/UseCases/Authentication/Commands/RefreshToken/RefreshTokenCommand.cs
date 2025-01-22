using Shared.Core.CQRS;
using Users.Application.Dto;

namespace Users.Application.UseCases.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand(string Token) : ICommand<TokensDto>;