using Shared.Core.Validation.Result;

namespace Shared.Core.Validation.FluentValidation;

/// <summary>
/// Marker for Fluent Validation library error
/// </summary>
/// <param name="Message">Error message</param>
/// <param name="Description">Error desctription</param>
public record FluentValidationError(string Message, string Description): Error (Message, Description);