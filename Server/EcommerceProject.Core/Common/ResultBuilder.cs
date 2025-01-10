namespace EcommerceProject.Core.Common;

public class ResultBuilder<TResult>
    where TResult : Result
{
    private TResult _result;
    private bool _continueValidation = true;

    public ResultBuilder(TResult result)
    {
        if(result.IsFailure)
            throw new ArgumentException($"ResultBuilder cannot be created with failed result.");
        _result = result;
    }
    
    public ResultBuilder<TResult> CheckError(bool errorCondition, Error error)
    {
        if(errorCondition && _continueValidation)
        {
            _result.Fail();
            _result.AddError(error);
        }
        return this;
    }
    

    public ResultBuilder<TResult> CheckErrorIf(bool checkCondition, bool errorCondition, Error error)
    {
        if(checkCondition)
            return CheckError(errorCondition, error);
        return this;
    }
    
    public ResultBuilder<TResult> CheckError(Func<bool> errorConditionFunc, Error error)
    {
        if (_continueValidation)
        {
            return CheckError(errorConditionFunc(), error);
        }
        return this;
    }

    public ResultBuilder<TResult> CheckErrorIf(bool checkCondition, Func<bool> errorConditionFunc, Error error)
    {
        if (checkCondition && _continueValidation)
        {
            return CheckError(errorConditionFunc(), error);
        }
        return this;
    }

    public async Task<ResultBuilder<TResult>> CheckErrorAsync(Func<Task<bool>> errorConditionFunc, Error error)
    {
        if (_continueValidation)
        {
            bool errorCondition = await errorConditionFunc();
            return CheckError(errorCondition, error);
        }
        return this;
    }

    public async Task<ResultBuilder<TResult>> CheckErrorIfAsync(
        bool checkCondition, 
        Func<Task<bool>> errorConditionFunc, 
        Error error)
    {
        if (checkCondition && _continueValidation)
        {
            bool errorCondition = await errorConditionFunc();
            return CheckError(errorCondition, error);
        }
        return this;
    }
    
    public ResultBuilder<TResult> DropIfFailed()
    {
        if(_result.IsFailure)
            _continueValidation = false;
        return this;
    }
    
    public TResult Build() => _result;
}