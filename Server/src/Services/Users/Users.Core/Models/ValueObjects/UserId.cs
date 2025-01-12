using Shared.Core.Validation;

namespace Users.Core.Models.ValueObjects;

public record UserId
{
    private UserId(Guid userId)
    {
        Value = userId;
    }

    public Guid Value { get; set; }

    public static Result<UserId> Create(Guid value)
    {
        if(value == Guid.Empty)
            return new Error("User id cannot be empty", nameof(value));
        
        return new UserId(value);
    }
}
