using System;

namespace EcommerceProject.Core.Models.Users.ValueObjects;

public record UserId
{
    private UserId(Guid userId)
    {
        Value = Guid.NewGuid();
    }

    public Guid Value { get; set; }

    public static UserId Create(Guid value)
    {
        return new UserId(value);
    }
}
