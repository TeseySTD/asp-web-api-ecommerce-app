using System.Collections.ObjectModel;

namespace Shared.Core.Validation;

public class Result
{
    protected List<Error> _errors = new ();

    public Result(bool isSuccess, IEnumerable<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors.ToList();
    }

    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;

    public IReadOnlyCollection<Error> Errors
    {
        get => new ReadOnlyCollection<Error>(_errors);
        private set => _errors = value.ToList();
    }

    public static Result Success() => new(true, [Error.None]);
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors);
    public static Result Failure(Error error) => new(false, [error]);
    
    public void Fail() => IsSuccess = false;
    public static ResultBuilder<Result> TryFail() => new(
        new Result(true, Array.Empty<Error>()));
    
    public Result AddError(Error error)
    {
        if(IsFailure)
            _errors.Add(error);
        return this;
    }
    
    public TResult Map<TResult>(Func<TResult> onSuccess, Func<IEnumerable<Error>, TResult> onFailure) => IsSuccess 
        ? onSuccess() 
        : onFailure(Errors!);
    
    public static implicit operator Result(Error error) => Failure(error); 
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
    public new static Result<TResponse> Failure(Error error) =>
        new(false, [error], default!);

    public new static ResultBuilder<Result<TResponse>> TryFail() => new(
        new Result<TResponse>(true, Array.Empty<Error>(), default!));
    
    public static ResultBuilder<Result<TResponse>> TryFail(TResponse value) => new(
        new Result<TResponse>(true, Array.Empty<Error>(), value));

    public static implicit operator Result<TResponse>(TResponse response) => Success(response);
    public static implicit operator Result<TResponse>(Error error) => Failure(error); 
    public TResult Map<TResult>(Func<TResponse, TResult> onSuccess, Func<IEnumerable<Error>, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value!) : onFailure(Errors!);
    }
}


