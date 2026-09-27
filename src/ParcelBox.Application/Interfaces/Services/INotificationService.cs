using ParcelBox.Application.Enums;
using ParcelBox.Domain.Parcels;

namespace ParcelBox.Application.Interfaces.Services;

public interface INotificationService
{
    Task<PickupMessageStatus> SendPickupCodeAsync(
        Parcel parcel,
        string pickupCode,
        CancellationToken cancellationToken);
}
