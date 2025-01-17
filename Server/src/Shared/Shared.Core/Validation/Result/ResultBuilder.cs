namespace Shared.Core.Validation.Result;

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
        {
            return Check(errorConditionFunc(), error);
        }

        return this;
    }

    public ResultBuilder<TResult> CheckIf(bool checkCondition, Func<bool> errorConditionFunc, Error error)
    {
        if (checkCondition && _continueValidation)
        {
            return Check(errorConditionFunc(), error);
        }

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
            .SelectMany(r => r.Errors).ToList();
        if (failures.Any())
        {
            _result.Fail();
            foreach (var e in failures)
            {
                _result.AddError(e);
            }
        }

        return this;
    }

    public ResultBuilder<TResult> DropIfFailed()
    {
        if (_result.IsFailure)
            _continueValidation = false;
        return this;
    }

    public TResult Build() => _result;
}