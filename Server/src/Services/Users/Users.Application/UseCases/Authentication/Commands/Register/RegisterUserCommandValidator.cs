using FluentValidation;
using Shared.Core.Validation.FluentValidation;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Authentication.Commands.Register;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Value.Name).MustBeCreatedWith(UserName.Create);
        
        RuleFor(x => x.Value.Password).MustBeCreatedWith(Password.Create);
        
        RuleFor(x => x.Value.Email).MustBeCreatedWith(Email.Create);
        
        RuleFor(x => x.Value.PhoneNumber).MustBeCreatedWith(PhoneNumber.Create);
        
        RuleFor(x => x.Value.Role).IsEnumName(typeof(User.UserRole));
    }
}