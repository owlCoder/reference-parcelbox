using ParcelBox.Application.DTOs.External;
using ParcelBox.Application.DTOs.Notifications;
using ParcelBox.Application.Enums;
using ParcelBox.Application.Interfaces.External;
using ParcelBox.Application.Interfaces.Services;

namespace ParcelBox.Application.Services;

public sealed class NotificationService : INotificationService
{
    private readonly IMessageGateway _messageGateway;

    public NotificationService(IMessageGateway messageGateway)
    {
        _messageGateway = messageGateway;
    }

    public async Task<PickupMessageStatus> SendPickupCodeAsync(
        PickupNotification notification,
        CancellationToken cancellationToken)
    {
        var message = new OutboundMessage(
            notification.RecipientPhone,
            $"Parcel {notification.TrackingCode} is ready. Pickup code: {notification.PickupCode}");

        var result = await _messageGateway.SendAsync(message, cancellationToken);

        return result.IsSuccess
            ? PickupMessageStatus.Delivered
            : PickupMessageStatus.Failed;
    }
}
