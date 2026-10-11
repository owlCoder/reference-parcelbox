using ParcelBox.Domain.Common.Enums;

namespace ParcelBox.Application.DTOs.Parcels;

public sealed class RegisterParcelInput
{
    public string TrackingCode { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public SizeCategory Size { get; set; };

    public RegisterParcelInput()
    {
    }

    public RegisterParcelInput(string trackingCode, string recipientPhone, SizeCategory size)
    {
        TrackingCode = trackingCode;
        RecipientPhone = recipientPhone;
        Size = size;
    }
}
