
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto;
using EcommerceProject.Core.Models.Users.Entities;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Login;

public record LoginUserCommand(string Email, string Password): ICommand<TokensDto>;