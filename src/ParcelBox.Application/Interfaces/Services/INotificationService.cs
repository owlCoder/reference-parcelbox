using ParcelBox.Application.DTOs.Notifications;
using ParcelBox.Application.Enums;

namespace ParcelBox.Application.Interfaces.Services;

public interface INotificationService
{
    Task<PickupMessageStatus> SendPickupCodeAsync(PickupNotificationDto notification, CancellationToken cancellationToken);
}
