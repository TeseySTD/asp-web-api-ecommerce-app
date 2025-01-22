using FluentValidation;
using Shared.Core.Validation.FluentValidation;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Authentication.Commands.Login;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Password).MustBeCreatedWith(Password.Create);
        
        RuleFor(x => x.Email).MustBeCreatedWith(Email.Create);
    }
}