using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Core.Models.Users.ValueObjects;
using FluentValidation;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Login;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Password).MustBeCreatedWith(Password.Create);
        
        RuleFor(x => x.Email).MustBeCreatedWith(Email.Create);
    }
}