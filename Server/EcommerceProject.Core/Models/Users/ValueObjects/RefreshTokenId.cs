using EcommerceProject.Core.Common;

namespace EcommerceProject.Core.Models.Users.ValueObjects;

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