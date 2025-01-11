using FluentValidation;
using FluentValidation.Results;

namespace Shared.Core.Validation.FluentValidation;

public static class CustomValidators
{
    // To validate using factory method
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
                    $" {error.Message}: {error.Description}");
                context.AddFailure(failure);
            }
        });
    }
}