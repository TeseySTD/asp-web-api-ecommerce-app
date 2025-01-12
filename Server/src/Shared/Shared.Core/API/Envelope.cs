using System.Text.Json;
using System.Text.Json.Serialization;
using Shared.Core.Validation;
using Shared.Core.Validation.FluentValidation;

namespace Shared.Core.API;

public class Envelope
{
    private readonly List<IEnvelopeError> _errors;

    public IReadOnlyList<IEnvelopeError> Errors => _errors.AsReadOnly();

    private Envelope(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        _errors = errors
            .GroupBy(e => e is FluentValidationError errorWithProperty
                ? errorWithProperty.Message
                : null)
            .Select(g =>
                g.Key is not null
                    ? (IEnvelopeError)new EnvelopeErrorWithPropertyName(g.Key, g.ToList())
                    : new DefaultEnvelopeError(g.ToList())
            )
            .ToList();
    }

    public static Envelope Of(Result result) => new(result.Errors);
    public static Envelope Of(IEnumerable<Error> errors) => new(errors);

    public static implicit operator Envelope(Result result) => Of(result);
    public static implicit operator Envelope(List<Error> errors) => Of(errors);
}


[JsonConverter(typeof(EnvelopeErrorConverter))]
public interface IEnvelopeError;

public record EnvelopeErrorWithPropertyName(string PropertyName, IEnumerable<Error> PropertyErrors) : IEnvelopeError;
public record DefaultEnvelopeError(IEnumerable<Error> SystemErrors) : IEnvelopeError;

// Custom JSON converter to handle polymorphic serialization
public class EnvelopeErrorConverter : JsonConverter<IEnvelopeError>
{
    public override IEnvelopeError Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("Deserialization is not supported.");
    }

    public override void Write(Utf8JsonWriter writer, IEnvelopeError value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case EnvelopeErrorWithPropertyName errorWithProperty:
                JsonSerializer.Serialize(writer, errorWithProperty, options);
                break;
            case DefaultEnvelopeError defaultError:
                JsonSerializer.Serialize(writer, defaultError, options);
                break;
            default:
                throw new JsonException("Unknown type of IEnvelopeError.");
        }
    }
}