using ParcelBox.Application.Common.Results;
using ParcelBox.Application.DTOs.External;
using ParcelBox.Application.Enums;

namespace ParcelBox.Application.Interfaces.External;

public interface IMessageGateway
{
    Task<Result<MessageGatewayError>> SendAsync(
        OutboundMessage message,
        CancellationToken cancellationToken);
}
