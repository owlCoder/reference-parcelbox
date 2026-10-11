namespace ParcelBox.Web.Common.Results;

public sealed class ClientResult<T> where T : class, new()
{
    public bool IsSuccess { get; private init; }
    public bool IsFailure => !IsSuccess;
    public T Value { get; private init; } = new();
    public string Error { get; private init; } = string.Empty;

    public static ClientResult<T> Success(T value) => new() { IsSuccess = true, Value = value };

    public static ClientResult<T> Failure(string error) => new() { Error = error };
}
