using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Core.Models.Users.ValueObjects;
using FluentValidation;

namespace EcommerceProject.Application.UseCases.Users.Commands.DeleteUser;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id).MustBeCreatedWith(UserId.Create);
    }
}