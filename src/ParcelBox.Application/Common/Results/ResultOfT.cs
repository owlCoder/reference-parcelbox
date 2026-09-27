namespace ParcelBox.Application.Common.Results;

public sealed class Result<TValue, TError>
    where TError : struct, Enum
{
    private Result(bool isSuccess, TValue value, TError error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public TValue Value { get; }
    public TError Error { get; }

    public static Result<TValue, TError> Success(TValue value)
    {
        return new Result<TValue, TError>(true, value, default);
    }

    public static Result<TValue, TError> Failure(TError error)
    {
        return new Result<TValue, TError>(false, default!, error);
    }
}
