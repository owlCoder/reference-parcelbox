namespace ParcelBox.Application.Common.Results;

public sealed class Result<TError>
    where TError : struct, Enum
{
    private Result(bool isSuccess, TError error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public TError Error { get; }

    public static Result<TError> Success()
    {
        return new Result<TError>(true, default);
    }

    public static Result<TError> Failure(TError error)
    {
        return new Result<TError>(false, error);
    }
}
