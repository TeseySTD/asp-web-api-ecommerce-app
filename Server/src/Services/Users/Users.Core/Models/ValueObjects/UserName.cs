using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Users.Core.Models.ValueObjects;

public record UserName
{
    public const int MaxNameLength = 100;
    public const int MinNameLength = 1;

    public string Value { get; init; }

    private UserName(string name) => Value = name;

    public static Result<UserName> Create(string name)
    {
        return Result<UserName>.Try(new UserName(name))
            .Check(string.IsNullOrEmpty(name),
                new Error("Name is required", "Name cannot be null or empty."))
            .DropIfFail()
            .Check(name.Length < MinNameLength || name.Length > MaxNameLength,
                new Error("Name is out of range.", $"Name must be between {MinNameLength} and {MaxNameLength}"))
            .Build();
    }
}