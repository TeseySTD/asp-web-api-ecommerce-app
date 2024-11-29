namespace EcommerceProject.Core.Models.Users.ValueObjects;

public record UserName
{
    public const int MaxNameLength = 100;
    public const int MinNameLength = 1;
    
    public string Value { get; init; }

    private UserName(string name) => Value = name;

    public static UserName Of(string name)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, MinNameLength, nameof(name));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, MaxNameLength, nameof(name));
        return new UserName(name);
    }
    
}