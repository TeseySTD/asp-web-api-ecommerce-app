using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.RefreshToken;

public record RefreshTokenCommand(string Token) : ICommand<TokensDto>;