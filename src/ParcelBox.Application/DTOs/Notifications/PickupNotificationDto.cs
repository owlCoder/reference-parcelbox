namespace ParcelBox.Application.DTOs.Notifications;

public sealed class PickupNotificationDto
{
    public string RecipientPhone { get; set; } = string.Empty;
    public string TrackingCode { get; set; } = string.Empty;
    public string PickupCode { get; set; } = string.Empty;

    public PickupNotificationDto()
    {
    }

    public PickupNotificationDto(string recipientPhone, string trackingCode, string pickupCode)
    {
        RecipientPhone = recipientPhone;
        TrackingCode = trackingCode;
        PickupCode = pickupCode;
    }
}
