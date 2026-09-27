namespace ParcelBox.Application.Common;

public sealed record OperationResult(bool IsSuccess, string? Error = null)
{
    public static OperationResult Success() => new(true);
    public static OperationResult Failure(string error) => new(false, error);
}
