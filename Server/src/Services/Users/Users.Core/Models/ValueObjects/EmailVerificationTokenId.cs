using Shared.Core.Validation.Result;

namespace Users.Core.Models.ValueObjects;

public record EmailVerificationTokenId
{
    public Guid Value { get; init; }

    private EmailVerificationTokenId(Guid value)
    {
        Value = value;
    }

    public static Result<EmailVerificationTokenId> Create(Guid Id)
    {
        if (Id != Guid.Empty)
            return new EmailVerificationTokenId(Id);
        return new Error(nameof(EmailVerificationTokenId), "Email verification token id can't be empty");
    }
}