using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Common;
using FluentValidation;
using MediatR;


namespace EcommerceProject.Application.Common.Classes.Validation;

public class ValidationBehaviour<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommandBase
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResults =
            await Task.WhenAll(_validators.Select(x => x.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .Where(x => x.Errors.Count != 0)
            .SelectMany(x => x.Errors)
            .ToList();

        var implementedError = new Error(
            "Validation error",
            string.Join("\n", errors.Select(x => x.ErrorMessage))
        );
        
        if (errors.Any())
        {
            //Check if Result type is generic
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var genericArgument = typeof(TResponse).GetGenericArguments()[0]; 
                var resultType = typeof(Result<>).MakeGenericType(genericArgument); 
                var errorResult = Activator.CreateInstance(resultType, false, implementedError, null);
                return (TResponse)errorResult!;
            }
            else if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(implementedError);
            }
        }

        return await next();
    }
}