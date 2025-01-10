namespace EcommerceProject.Core.Common;

public static class ResultBuilderExtensions
{
    public static async Task<ResultBuilder<TResult>> CheckError<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder, bool errorCondition, Error error) where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;
        resultBuilderAfter.CheckError(errorCondition, error);

        return resultBuilderAfter;
    }


    public static async Task<ResultBuilder<TResult>> CheckErrorIf<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder,
        bool checkCondition, bool errorCondition, Error error) where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;
        if (checkCondition)
            resultBuilderAfter.CheckError(errorCondition, error);

        return resultBuilderAfter;
    }

    public static async Task<ResultBuilder<TResult>> CheckError<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder, Func<bool> errorConditionFunc, Error error)
        where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;
        resultBuilderAfter.CheckError(errorConditionFunc(), error);

        return resultBuilderAfter;
    }

    public static async Task<ResultBuilder<TResult>> CheckErrorIf<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder, bool checkCondition, Func<bool> errorConditionFunc,
        Error error) where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;

        resultBuilderAfter.CheckErrorIf(checkCondition, errorConditionFunc(), error);

        return resultBuilderAfter;
    }

    public static async Task<ResultBuilder<TResult>> CheckErrorAsync<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder, Func<Task<bool>> errorConditionFunc, Error error)
        where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;

        return await resultBuilderAfter.CheckErrorAsync(errorConditionFunc, error);
    }

    public static async Task<ResultBuilder<TResult>> CheckErrorIfAsync<TResult>(
        this Task<ResultBuilder<TResult>> resultBuilder, bool checkCondition,
        Func<Task<bool>> errorConditionFunc,
        Error error) where TResult : Result
    {
        var resultBuilderAfter = await resultBuilder;

        return await resultBuilderAfter.CheckErrorIfAsync(checkCondition, errorConditionFunc, error);
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