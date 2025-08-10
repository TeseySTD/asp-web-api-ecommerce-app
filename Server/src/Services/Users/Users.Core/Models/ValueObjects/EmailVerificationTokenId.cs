using Shared.Core.Validation.Result;

namespace Users.Core.Models.ValueObjects;

public record EmailVerificationTokenId
{
    public Guid Value { get; init; }

    private EmailVerificationTokenId(Guid value)
    {
        Value = value;
    }

    public static Result<EmailVerificationTokenId> Create(Guid id)
    {
        return Result<EmailVerificationTokenId>.Try(new EmailVerificationTokenId(id))
            .Check(id == Guid.Empty, new IdRequiredError())
            .Build();
    }

    public sealed record IdRequiredError()
        : Error("Refresh token id cannot be empty", "RefreshTokenId value object failure");
}