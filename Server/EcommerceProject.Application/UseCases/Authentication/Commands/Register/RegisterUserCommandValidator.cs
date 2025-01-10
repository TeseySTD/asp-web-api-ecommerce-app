using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;
using FluentValidation;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Register;

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