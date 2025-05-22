using FluentValidation;
using FluentValidation.Results;
using Shared.Core.Validation.Result;

namespace Shared.Core.Validation.FluentValidation;

public static class CustomValidators
{
    /// <summary>
    /// Validates a property by attempting to create an object using a factory method.
    /// </summary>
    /// <remarks>
    /// This validator is ideal for value objects, that incapsulate validation logic in factory methi
    /// </remarks>
    /// <typeparam name="TElement">The type of the property being validated.</typeparam>
    /// <typeparam name="TObject">The type of the object to be created by the factory.</typeparam>
    /// <typeparam name="T">The type of the root object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the property.</param>
    /// <param name="factoryMethod">
    /// A factory method that attempts to create an object of type TObject from the provided TElement.
    /// Returns a Result object with validation errors if the creation fails.
    /// </param>
    /// <returns>A rule builder options object to continue building the validation rule.</returns>
    /// <example>
    /// RuleFor(x => x.Element)
    ///     .MustBeCreatedWith(value => MyFactory.Create(value));
    /// </example>
    public static IRuleBuilderOptionsConditions<T, TElement> MustBeCreatedWith<T, TElement, TObject>(
        this IRuleBuilder<T, TElement> ruleBuilder,
        Func<TElement, Result<TObject>> factoryMethod)
        where TObject : notnull
    {
        return ruleBuilder.Custom((value, context) =>
        {
            Result<TObject> result = factoryMethod(value);

            if (result.IsSuccess)
                return;

            foreach (var error in result.Errors)
            {
                var propertyName = context.PropertyPath;
                var failure = new ValidationFailure(propertyName,
                    $"{error.Message}: {error.Description}");
                context.AddFailure(failure);
            }
        });
    }
}