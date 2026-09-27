namespace ParcelBox.Application.DTOs.Notifications;

public sealed record PickupNotification(
    string RecipientPhone,
    string TrackingCode,
    string PickupCode);
