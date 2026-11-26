using ParcelBox.Application.Enums;

namespace ParcelBox.Application.DTOs.Pickup;

public sealed class PickupCodeReissueResultDto
{
    public string TrackingCode { get; set; } = string.Empty;
    public PickupMessageStatus MessageDelivery { get; set; }

    public PickupCodeReissueResultDto()
    {
    }

    public PickupCodeReissueResultDto(string trackingCode, PickupMessageStatus messageDelivery)
    {
        TrackingCode = trackingCode;
        MessageDelivery = messageDelivery;
    }
}
