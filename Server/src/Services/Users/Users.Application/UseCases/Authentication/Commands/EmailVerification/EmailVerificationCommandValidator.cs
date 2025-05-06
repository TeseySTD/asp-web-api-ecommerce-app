using FluentValidation;
using Shared.Core.Validation.FluentValidation;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Authentication.Commands.EmailVerification;

public class EmailVerificationCommandValidator : AbstractValidator<EmailVerificationCommand>
{
    public EmailVerificationCommandValidator()
    {
        RuleFor(x => x.Id).MustBeCreatedWith(EmailVerificationTokenId.Create);
    }
}