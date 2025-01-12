using FluentValidation;
using Shared.Core.Validation.FluentValidation;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Authentication.Commands.Logout;

public class LogoutUserCommandValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserCommandValidator()
    {
        RuleFor(x => x.UserId.Value).MustBeCreatedWith(UserId.Create);
    }
}