using System;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Core.Models.Users.ValueObjects;

public record UserId
{
    private UserId(Guid userId)
    {
        Value = Guid.NewGuid();
    }

    public Guid Value { get; set; }

    public static Result<UserId> Create(Guid value)
    {
        if(value == Guid.Empty)
            return new Error("User id cannot be empty", nameof(value));
        
        return new UserId(value);
    }
}
