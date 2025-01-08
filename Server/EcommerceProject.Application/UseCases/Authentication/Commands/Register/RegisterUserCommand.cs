using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto;
using EcommerceProject.Application.Dto.User;
using EcommerceProject.Core.Models.Users.Entities;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Register;

public record RegisterUserCommand(UserWriteDto Value) : ICommand<TokensDto>;