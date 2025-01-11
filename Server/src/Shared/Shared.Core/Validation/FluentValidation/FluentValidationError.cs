namespace Shared.Core.Validation.FluentValidation;

// Marker for fluent validation error
public record FluentValidationError(string Message, string Description): Error (Message, Description);