using ParcelBox.Application.DTOs.External;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.External;
using ParcelBox.Application.Interfaces.Services;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Services;

public sealed class NotificationService : INotificationService
{
    private readonly IMessageGateway _messageGateway;

    public NotificationService(IMessageGateway messageGateway)
    {
        _messageGateway = messageGateway;
    }

    public async Task<PickupMessageStatus> SendPickupCodeAsync(
        Parcel parcel,
        string pickupCode,
        CancellationToken cancellationToken)
    {
        var message = new OutboundMessage(
            parcel.RecipientPhone,
            $"Parcel {parcel.TrackingCode} is ready. Pickup code: {pickupCode}");

        var result = await _messageGateway.SendAsync(message, cancellationToken);

        return result.IsSuccess
            ? PickupMessageStatus.Delivered
            : PickupMessageStatus.Failed;
    }
}
