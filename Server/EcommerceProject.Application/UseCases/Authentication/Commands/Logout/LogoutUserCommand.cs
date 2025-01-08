using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Logout;

public record LogoutUserCommand(UserId UserId) : ICommand;