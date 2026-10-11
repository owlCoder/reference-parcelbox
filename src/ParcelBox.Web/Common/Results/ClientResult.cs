namespace ParcelBox.Web.Common.Results;

public sealed class ClientResult
{
    public bool IsSuccess { get; private init; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; private init; } = string.Empty;

    public static ClientResult Success() => new() { IsSuccess = true };

    public static ClientResult Failure(string error) => new() { Error = error };
}
