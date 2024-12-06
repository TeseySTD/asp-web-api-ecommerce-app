namespace EcommerceProject.Core.Common;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }
    public bool IsSuccess { get; }
    public bool IsFailure  => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
}

public class Result<TResponse> : Result
    where TResponse : notnull
{
    public TResponse Value { get; }

    public Result(bool isSuccess, Error error, TResponse value) : base(isSuccess, error)
    {
        Value = value;
    }
    
    public static Result<TResponse> Success(TResponse response) => new(true, Error.None, response);
    public new static Result<TResponse> Failure(Error error) => new(false, error, default!);

}

