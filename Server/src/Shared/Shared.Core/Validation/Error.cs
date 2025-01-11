namespace Shared.Core.Validation;

public record Error(string Message, string Description)
{
    public static Error None => new ("", "");
    public static Error NotFound => new("Not Found", "Not Found error");
}