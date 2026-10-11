namespace ParcelBox.Application.DTOs.Notifications;

public sealed class PickupNotification
{
    public string RecipientPhone { get; set; } = string.Empty;
    public string TrackingCode { get; set; } = string.Empty;
    public string PickupCode { get; set; } = string.Empty;

    public PickupNotification()
    {
    }

    public PickupNotification(string recipientPhone, string trackingCode, string pickupCode)
    {
        RecipientPhone = recipientPhone;
        TrackingCode = trackingCode;
        PickupCode = pickupCode;
    }
}
