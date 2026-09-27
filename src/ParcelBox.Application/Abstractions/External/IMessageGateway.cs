using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Abstractions.External;

public interface IMessageGateway
{
    Task<Result<MessageGatewayError>> SendAsync(
        OutboundMessage message,
        CancellationToken cancellationToken);
}
