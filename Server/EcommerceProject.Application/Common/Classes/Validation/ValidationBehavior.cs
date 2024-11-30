using EcommerceProject.Application.Common.Interfaces.Messaging;
using FluentValidation;
using MediatR;


namespace EcommerceProject.Application.Common.Classes.Validation;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResults =
            await Task.WhenAll(_validators.Select(x => x.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .Where(x => x.Errors.Count != 0)
            .SelectMany(x => x.Errors)
            .ToList();

        if (errors.Any())
        {
            throw new ValidationException(errors);
        }

        return await next();
    }
}