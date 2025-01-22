namespace Shared.Core.Validation.Result;

public record Error(string Message, string Description)
{
    public static Error None => new ("", "");
    public static Error NotFound => new("Not Found", "Not Found error");
}