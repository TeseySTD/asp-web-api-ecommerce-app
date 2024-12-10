using System.Collections.ObjectModel;
using System.Runtime.InteropServices.JavaScript;

namespace EcommerceProject.Core.Common;

public class Result
{
    protected List<Error> _errors = new ();

    public Result(bool isSuccess, IEnumerable<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors.ToList();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public IReadOnlyCollection<Error> Errors
    {
        get => new ReadOnlyCollection<Error>(_errors);
        private set => _errors = value.ToList();
    }

    public static Result Success() => new(true, [Error.None]);
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors);
    public new static FailResultBuilder<Result> Fail() => new(
        new Result(false, Array.Empty<Error>()));
    
    public Result AddError(Error error)
    {
        _errors.Add(error);
        return this;
    }
}

public class Result<TResponse> : Result
    where TResponse : notnull
{
    public TResponse Value { get; }

    public Result(bool isSuccess, IEnumerable<Error> errors, TResponse value) : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<TResponse> Success(TResponse response) => new(true, [Error.None], response);

    public new static Result<TResponse> Failure(IEnumerable<Error> errors) =>
        new(false, errors.ToList(), default!);

    public new static FailResultBuilder<Result<TResponse>> Fail() => new(
        new Result<TResponse>(false, Array.Empty<Error>(), default!));

    public static implicit operator Result<TResponse>(TResponse response) => Success(response);

    public TResult Map<TResult>(Func<TResponse, TResult> onSuccess, Func<IEnumerable<Error>, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Errors!);
    }
}

public class FailResultBuilder<TResult>
    where TResult : Result
{
    private TResult _result;

    public FailResultBuilder(TResult result)
    {
        _result = result;
    }

    public FailResultBuilder<TResult> AddError(Error error)
    {
        _result.AddError(error);
        return this;
    }
    
    public TResult Build() => _result;
}