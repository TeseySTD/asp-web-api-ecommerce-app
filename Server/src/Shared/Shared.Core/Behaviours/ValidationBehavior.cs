using FluentValidation;
using MediatR;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.FluentValidation;
using Shared.Core.Validation.Result;

namespace Shared.Core.Behaviours;

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
            await Task.WhenAll(
                _validators
                    .Select(x => x.ValidateAsync(context, cancellationToken))
            );
        
        IEnumerable<Error> errors;
        errors = validationResults
            .Where(r => r.Errors.Count != 0)
            .SelectMany(r => r.Errors)
            .Select(f =>
                new FluentValidationError(
                    f.PropertyName.Replace("Value.", ""),
                    f.ErrorMessage
                )
            )
            .ToList();


        if (errors.Any())
        {
            //Check if Result type is generic
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var genericArgument = typeof(TResponse).GetGenericArguments()[0];
                var resultType = typeof(Result<>).MakeGenericType(genericArgument);
                var errorResult = Activator.CreateInstance(resultType, false, errors, null);
                return (TResponse)errorResult!;
            }
            else if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)Result.Failure(errors);
            }
        }

        return await next();
    }
}