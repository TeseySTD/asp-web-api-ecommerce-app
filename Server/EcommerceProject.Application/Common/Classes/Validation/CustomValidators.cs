using EcommerceProject.Core.Common;
using FluentValidation;

namespace EcommerceProject.Application.Common.Classes.Validation;

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
            
            if(result.IsSuccess)
                return;
            
            context.AddFailure(string.Join("\n",
                                result.Errors.Select(x => x.Message+ ":" + x.Description)));
        });
    }
        
}