using EcommerceProject.Core.Common;

namespace EcommerceProject.Core.Models.Users.ValueObjects;

public record UserName
{
    public const int MaxNameLength = 100;
    public const int MinNameLength = 1;

    public string Value { get; init; }

    private UserName(string name) => Value = name;

    public static Result<UserName> Create(string name)
    {
        return Result<UserName>.TryFail(new UserName(name))
            .CheckError(string.IsNullOrEmpty(name),
                new Error("Name is required", "Name cannot be null or empty."))
            .DropIfFailed()
            .CheckError(name.Length < MinNameLength || name.Length > MaxNameLength,
                new Error("Name is out of range.", $"Name must be between {MinNameLength} and {MaxNameLength}"))
            .Build();
    }
}