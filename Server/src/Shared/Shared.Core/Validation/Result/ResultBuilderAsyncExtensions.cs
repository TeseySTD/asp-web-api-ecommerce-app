namespace Shared.Core.Validation.Result;

public static class ResultBuilderAsyncExtensions
{
    public static async Task<ResultBuilder<TResult>> Check<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder, bool errorCondition, Error error) where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;
        resultBuilderAfter.Check(errorCondition, error);

        return resultBuilderAfter;
    }


    public static async Task<ResultBuilder<TResult>> CheckIf<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder,
        bool checkCondition, bool errorCondition, Error error) where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;
        if (checkCondition)
            resultBuilderAfter.Check(errorCondition, error);

        return resultBuilderAfter;
    }

    public static async Task<ResultBuilder<TResult>> Check<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder, Func<bool> errorConditionFunc, Error error)
        where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;
        resultBuilderAfter.Check(errorConditionFunc(), error);

        return resultBuilderAfter;
    }

    public static async Task<ResultBuilder<TResult>> CheckIf<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder, bool checkCondition, Func<bool> errorConditionFunc,
        Error error) where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;

        resultBuilderAfter.CheckIf(checkCondition, errorConditionFunc(), error);

        return resultBuilderAfter;
    }

    public static async Task<ResultBuilder<TResult>> CheckAsync<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder, Func<Task<bool>> errorConditionFunc, Error error)
        where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;

        return await resultBuilderAfter.CheckAsync(errorConditionFunc, error);
    }

    public static async Task<ResultBuilder<TResult>> CheckIfAsync<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder, bool checkCondition,
        Func<Task<bool>> errorConditionFunc,
        Error error) where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;

        return await resultBuilderAfter.CheckIfAsync(checkCondition, errorConditionFunc, error);
    }

    public static async Task<ResultBuilder<TResult>> DropIfFailed<TResult>(this Task<ResultBuilder<TResult>> resultBuilder)
        where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;
        return resultBuilderAfter.DropIfFailed();
    }

    public static async Task<TResult> BuildAsync<TResult>(this Task<ResultBuilder<TResult>> resultBuilder)
        where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;
        return resultBuilderAfter.Build();
    }
}