namespace ParcelBox.Application.Common;

public sealed record OperationResult<T>(bool IsSuccess, T? Value = default, string? Error = null)
{
    public static OperationResult<T> Success(T value) => new(true, value);
    public static OperationResult<T> Failure(string error) => new(false, default, error);
}
