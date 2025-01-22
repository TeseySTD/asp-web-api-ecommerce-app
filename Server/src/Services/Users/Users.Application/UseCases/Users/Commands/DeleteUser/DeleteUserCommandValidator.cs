using FluentValidation;
using Shared.Core.Validation.FluentValidation;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Users.Commands.DeleteUser;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.Id).MustBeCreatedWith(UserId.Create);
    }
}