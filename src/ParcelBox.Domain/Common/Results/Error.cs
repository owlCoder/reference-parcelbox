namespace ParcelBox.Domain.Common.Results;

public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error None { get; } = new(string.Empty, string.Empty, ErrorType.None);
}
