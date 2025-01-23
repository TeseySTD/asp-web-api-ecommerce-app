using System.Collections.ObjectModel;

namespace Shared.Core.Validation.Result;

/// <summary>
/// Represents the result of an operation with potential errors.
/// </summary>
/// <remarks>
/// A result can either indicate success or failure. 
/// If failed, the <see cref="Errors"/> property will contain a list of validation errors.
/// </remarks>
public class Result
{
    protected readonly List<Error> _errors = new();
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;

    public IReadOnlyCollection<Error> Errors => new ReadOnlyCollection<Error>(_errors);

    protected Result(bool isSuccess, IEnumerable<Error> errors)
    {
        IsSuccess = isSuccess;
        if (errors != null)
            _errors.AddRange(errors);
    }

    public static Result Success() => new(true, new[] { Error.None });
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors);
    public static Result Failure(Error error) => Failure(new[] { error });

    public Result AddError(Error error)
    {
        if (IsFailure)
            _errors.Add(error);
        return this;
    }

    public TResult Map<TResult>(Func<TResult> onSuccess, Func<IEnumerable<Error>, TResult> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Errors);

    public static implicit operator Result(Error error) => Failure(error);

    public static ResultBuilder<Result> Try() => new(Result.Success());

    public void Fail() => IsSuccess = false;
}

/// <summary>
/// Represents the result of an operation with a response value.
/// </summary>
/// <remarks>
/// A result can either indicate success or failure. 
/// If successful, the <see cref="Value"/> property will contain the result of the operation.
/// If failed, the <see cref="Value"/> property will contain a default value.
/// </remarks>
public class Result<TResponse> : Result where TResponse : notnull
{
    public TResponse Value { get; }

    private Result(bool isSuccess, IEnumerable<Error> errors, TResponse value)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<TResponse> Success(TResponse response) => new(true, new[] { Error.None }, response);
    public new static Result<TResponse> Failure(IEnumerable<Error> errors) => new(false, errors, default!);
    public new static Result<TResponse> Failure(Error error) => Failure(new[] { error });

    public new static ResultBuilder<Result<TResponse>> Try() => new(Success(default!));
    public static ResultBuilder<Result<TResponse>> Try(TResponse value) => new(Success(value));

    public TResult Map<TResult>(Func<TResponse, TResult> onSuccess, Func<IEnumerable<Error>, TResult> onFailure)
        => IsSuccess ? onSuccess(Value) : onFailure(Errors);

    public static implicit operator Result<TResponse>(TResponse response) => Success(response);
    public static implicit operator Result<TResponse>(Error error) => Failure(error);
}