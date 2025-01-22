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

    public static Result<RefreshTokenId> Create(Guid Id)
    {
        if (Id != Guid.Empty)
            return new RefreshTokenId(Id);
        return new Error(nameof(RefreshTokenId), "Refresh token id can't be empty");
    }
};