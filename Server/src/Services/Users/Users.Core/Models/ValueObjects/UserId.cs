using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Users.Core.Models.ValueObjects;

public record UserId
{
    private UserId(Guid userId)
    {
        Value = userId;
    }

    public Guid Value { get; set; }

    public static Result<UserId> Create(Guid productId)
    {
        return Result<UserId>.Try(new UserId(productId))
            .Check(productId == Guid.Empty, new IdRequiredError())
            .Build();
    }

    public sealed record IdRequiredError() : Error("User Id cannot be empty", "UserId value object failure");
}