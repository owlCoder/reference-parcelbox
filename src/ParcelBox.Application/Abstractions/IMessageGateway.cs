namespace ParcelBox.Application.Abstractions;

public interface IMessageGateway
{
    Task<bool> SendPickupCodeAsync(string destination, string trackingCode, string pickupCode, CancellationToken cancellationToken);
}
