using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Users.Core.Models.ValueObjects;

public record RefreshTokenId
{
    public Guid Value { get; init; }

    private RefreshTokenId(Guid value)
    {
        Value = value;
    }

    public static Result<RefreshTokenId> Create(Guid id)
    {
        return Result<RefreshTokenId>.Try(new RefreshTokenId(id))
            .Check(id == Guid.Empty, new IdRequiredError())
            .Build();
    }

    public sealed record IdRequiredError()
        : Error("Refresh token id cannot be empty", "RefreshTokenId value object failure");
};