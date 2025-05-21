namespace Shared.Core.Validation.Result;

public static class ResultBuilderStringParamsExtensions
{
    public static ResultBuilder<TResult> Check<TResult>(this ResultBuilder<TResult> resultBuilder,
        bool errorCondition,
        string message,
        string description)
        where TResult : Result =>
        resultBuilder.Check(errorCondition, new(message, description));

    public static ResultBuilder<TResult> CheckIf<TResult>(this ResultBuilder<TResult> resultBuilder,
        bool checkCondition, bool errorCondition,
        string message,
        string description)
        where TResult : Result =>
        resultBuilder.CheckIf(checkCondition, errorCondition, new(message, description));

    public static ResultBuilder<TResult> Check<TResult>(this ResultBuilder<TResult> resultBuilder,
        Func<bool> errorConditionFunc,
        string message,
        string description)
        where TResult : Result =>
        resultBuilder.Check(errorConditionFunc, new(message, description));

    public static ResultBuilder<TResult> CheckIf<TResult>(this ResultBuilder<TResult> resultBuilder,
        bool checkCondition, Func<bool> errorConditionFunc,
        string message,
        string description)
        where TResult : Result =>
        resultBuilder.CheckIf(checkCondition, errorConditionFunc, new(message, description));

    public static Task<ResultBuilder<TResult>> CheckAsync<TResult>(this ResultBuilder<TResult> resultBuilder,
        Func<Task<bool>> errorConditionFunc,
        string message,
        string description)
        where TResult : Result =>
        resultBuilder.CheckAsync(errorConditionFunc, new(message, description));

    public static Task<ResultBuilder<TResult>> CheckIfAsync<TResult>(
        this ResultBuilder<TResult> resultBuilder,
        bool checkCondition,
        Func<Task<bool>> errorConditionFunc,
        string message,
        string description)
        where TResult : Result =>
        resultBuilder.CheckIfAsync(checkCondition, errorConditionFunc, new(message, description));
}