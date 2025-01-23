namespace Shared.Core.Validation.Result;
/// <summary>
/// Record for <see cref="Result"/> validation error
/// </summary>
/// <param name="Message">Error message</param>
/// <param name="Description">Error desctription</param>
public record Error(string Message, string Description)
{
    public static Error None => new ("", "");
    public static Error NotFound => new("Not Found", "Not Found error");
}