using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Abstractions.External;

public static class MessageGatewayErrors
{
    public static Error Unavailable { get; } = new(
        "MessageGateway.Unavailable",
        "Message gateway is unavailable.",
        ErrorType.Unavailable);
}
