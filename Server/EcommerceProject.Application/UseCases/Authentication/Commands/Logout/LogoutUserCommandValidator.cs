using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Core.Models.Users.ValueObjects;
using FluentValidation;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Logout;

public class LogoutUserCommandValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator()
    {
        RuleFor(x => x.UserId.Value).MustBeCreatedWith(UserId.Create);
    }
}