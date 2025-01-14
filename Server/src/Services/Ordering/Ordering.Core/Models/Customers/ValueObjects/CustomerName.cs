using Shared.Core.Validation;

namespace Ordering.Core.Models.Customers.ValueObjects;

public record CustomerName
{
    public const int MaxNameLength = 100;
    public const int MinNameLength = 1;

    public string Value { get; init; }

    private CustomerName(string name) => Value = name;

    public static Result<CustomerName> Create(string name)
    {
        return Result<CustomerName>.TryFail(new CustomerName(name))
            .CheckError(string.IsNullOrEmpty(name),
                new Error("Name is required", "Name cannot be null or empty."))
            .DropIfFailed()
            .CheckError(name.Length < MinNameLength || name.Length > MaxNameLength,
                new Error("Name is out of range.", $"Name must be between {MinNameLength} and {MaxNameLength}"))
            .Build();
    }
}