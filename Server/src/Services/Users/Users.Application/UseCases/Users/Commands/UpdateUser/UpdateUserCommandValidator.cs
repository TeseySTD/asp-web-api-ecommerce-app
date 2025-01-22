using FluentValidation;
using Shared.Core.Auth;
using Shared.Core.Validation.FluentValidation;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Users.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).MustBeCreatedWith(UserId.Create);
        
        RuleFor(x => x.Value.Name).MustBeCreatedWith(UserName.Create);
        
        RuleFor(x => x.Value.Password).MustBeCreatedWith(Password.Create);
        
        RuleFor(x => x.Value.Email).MustBeCreatedWith(Email.Create);
        
        RuleFor(x => x.Value.PhoneNumber).MustBeCreatedWith(PhoneNumber.Create);
        
        RuleFor(x => x.Value.Role).IsEnumName(typeof(UserRole));
    }
}