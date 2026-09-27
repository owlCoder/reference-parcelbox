using ParcelBox.Application.Common.Results;

namespace ParcelBox.Application.Interfaces.External;

public interface IMessageGateway
{
    Task<Result<MessageGatewayError>> SendAsync(
        OutboundMessage message,
        CancellationToken cancellationToken);
}
