using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Abstractions.External;

public interface IMessageGateway
{
    Task<Result<MessageGatewayError>> SendPickupCodeAsync(
        string destination,
        string trackingCode,
        string pickupCode,
        CancellationToken cancellationToken);
}
