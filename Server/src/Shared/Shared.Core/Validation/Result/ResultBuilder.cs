namespace Shared.Core.Validation.Result;

/// <summary>
/// Helper class for building results with validation.
/// </summary>
/// <typeparam name="TResult">The type of the result value to be built.</typeparam>
/// <remarks>
/// This class provides a fluent API to construct a result object that may either
/// represent a successful outcome with a value or a failed one with validation errors.
/// </remarks>
/// <example>
/// Here's a simplified example of how to use the `ResultBuilder` for validation:
/// <code>
/// var result = Result.Try()
///     .Check(() => userInput.Length > 5, 
///         new Error("Validation error", "Input must be longer than 5 characters."))
///     .DropIfFail()
///     .Check(() => userInput.All(char.IsLetter), 
///         new Error("Validation error", "Input must contain only letters."))
///     .Build();
/// </code>
/// </example>
public class ResultBuilder<TResult>
    where TResult : Result
{
    private TResult _result;
    private bool _continueValidation = true;

    public ResultBuilder(TResult result)
    {
        if (result.IsFailure)
            throw new ArgumentException($"ResultBuilder cannot be created with failed result.");
        _result = result;
    }

    public ResultBuilder<TResult> Check(bool errorCondition, Error error)
    {
        if (errorCondition && _continueValidation)
        {
            _result.Fail();
            _result.AddError(error);
        }
        return this;
    }
    
    public ResultBuilder<TResult> CheckIf(bool checkCondition, bool errorCondition, Error error)
    {
        if (checkCondition)
            return Check(errorCondition, error);
        return this;
    }

    public ResultBuilder<TResult> Check(Func<bool> errorConditionFunc, Error error)
    {
        if (_continueValidation)
            return Check(errorConditionFunc(), error);
        return this;
    }

    public ResultBuilder<TResult> CheckIf(bool checkCondition, Func<bool> errorConditionFunc, Error error)
    {
        if (checkCondition && _continueValidation)
            return Check(errorConditionFunc(), error);
        return this;
    }

    public async Task<ResultBuilder<TResult>> CheckAsync(Func<Task<bool>> errorConditionFunc, Error error)
    {
        if (_continueValidation)
        {
            bool errorCondition = await errorConditionFunc();
            return Check(errorCondition, error);
        }
        return this;
    }

    public async Task<ResultBuilder<TResult>> CheckIfAsync(
        bool checkCondition,
        Func<Task<bool>> errorConditionFunc,
        Error error)
    {
        if (checkCondition && _continueValidation)
        {
            bool errorCondition = await errorConditionFunc();
            return Check(errorCondition, error);
        }
        return this;
    }

    public ResultBuilder<TResult> Combine(params TResult[] results)
    {
        var failures = results
            .Where(r => r.IsFailure)
            .SelectMany(r => r.Errors)
            .ToList();
        
        if (failures.Any())
        {
            _result.Fail();
            foreach (var e in failures)
                _result.AddError(e);
        }
        return this;
    }

    public ResultBuilder<TResult> DropIfFail()
    {
        if (_result.IsFailure)
            _continueValidation = false;
        return this;
    }

    public TResult Build() => _result;
}