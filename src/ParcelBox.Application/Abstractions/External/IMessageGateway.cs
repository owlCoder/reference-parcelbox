using ParcelBox.Domain.Common.Results;

namespace ParcelBox.Application.Abstractions.External;

public interface IMessageGateway
{
    Task<Result> SendPickupCodeAsync(
        string destination,
        string trackingCode,
        string pickupCode,
        CancellationToken cancellationToken);
}
