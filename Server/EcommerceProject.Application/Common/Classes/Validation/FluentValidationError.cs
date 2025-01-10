using System.Text.Json.Serialization;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Application.Common.Classes.Validation;

// Marker for fluent validation error
public record FluentValidationError(string Message, string Description): Error (Message, Description);